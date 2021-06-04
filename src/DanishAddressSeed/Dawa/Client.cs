using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DanishAddressSeed.Location;
using DanishAddressSeed.Mapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal class Client : IClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Client> _logger;
        private readonly ILocationPostgres _locationPostgres;
        private const string _dawaBasePath = "https://api.dataforsyningen.dk/replikering/udtraek";
        private readonly ILocationDawaMapper _locationDawaMapper;

        public Client(
            HttpClient httpClient,
            ILogger<Client> logger,
            ILocationPostgres locationPostgres,
            ILocationDawaMapper locationDawaMapper)
        {
            _httpClient = httpClient;
            _logger = logger;
            _locationPostgres = locationPostgres;
            _locationDawaMapper = locationDawaMapper;
        }

        public async Task ImportOfficalAccessAddress()
        {
            var postCodesTask = GetPostCodes();
            var roadsTask = GetRoads();

            var postCodes = await postCodesTask;
            var roads = await roadsTask;

            var accessAddressUrl = $"{_dawaBasePath}?entitet=adgangsadresse";
            var count = 0;

            using var response = await _httpClient.GetAsync(accessAddressUrl, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var addresses = new List<OfficalAccessAddress>();
            var serializer = new JsonSerializer();
            while (await reader.ReadAsync())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var address = serializer.Deserialize<DawaOfficalAccessAddress>(reader);

                    postCodes.TryGetValue(address.PostDistrictCode, out var postDistrictName);
                    roads.TryGetValue(address.RoadExternalId, out var roadName);

                    var mappedAddress = _locationDawaMapper.Map(address, postDistrictName, roadName);

                    addresses.Add(mappedAddress);

                    if (addresses.Count == 10000)
                    {
                        count += 10000;
                        await _locationPostgres.InsertOfficalAccessAddresses(addresses);
                        _logger.LogInformation($"Imported {nameof(DawaOfficalAccessAddress)}: {count}");

                        addresses.Clear();
                    }
                }
            }

            await _locationPostgres.InsertOfficalAccessAddresses(addresses);
        }

        public async Task ImportOfficalUnitAddress()
        {
            var unitAddressUrl = $"{_dawaBasePath}?entitet=adresse";

            var count = 0;

            using var response = await _httpClient.GetAsync(unitAddressUrl, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var addresses = new List<OfficalUnitAddress>();
            var serializer = new JsonSerializer();
            while (await reader.ReadAsync())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var unitAddress = serializer.Deserialize<DawaOfficalUnitAddress>(reader);

                    var mappedAddress = _locationDawaMapper.Map(unitAddress);

                    addresses.Add(mappedAddress);

                    if (addresses.Count == 10000)
                    {
                        count += 10000;
                        await _locationPostgres.InsertOfficialUnitAddresses(addresses);
                        _logger.LogInformation($"Imported {nameof(OfficalUnitAddress)}: {count}");

                        addresses.Clear();
                    }
                }
            }

            await _locationPostgres.InsertOfficialUnitAddresses(addresses);
        }

        private async Task<Dictionary<string, string>> GetRoads()
        {
            var serializer = new JsonSerializer();
            var postNumberUrl = $"{_dawaBasePath}?entitet=navngivenvej";
            var postNumberResponse = await _httpClient.GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead);

            var stream = await postNumberResponse.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var result = serializer.Deserialize<List<DawaRoad>>(reader);

            return result.ToDictionary(x => x.Id, x => x.Name);
        }

        private async Task<Dictionary<string, string>> GetPostCodes()
        {
            var serializer = new JsonSerializer();
            var postNumberUrl = $"{_dawaBasePath}?entitet=postnummer";
            var postNumberResponse = await _httpClient.GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead);

            var stream = await postNumberResponse.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var result = serializer.Deserialize<List<DawaPostCode>>(reader);

            return result.ToDictionary(x => x.Number, x => x.Name);
        }
    }
}
