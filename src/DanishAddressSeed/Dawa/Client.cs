using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DanishAddressSeed.Location;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal class Client : IClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Client> _logger;
        private readonly ILocationPostgres _locationPostgres;

        public Client(
            HttpClient httpClient,
            ILogger<Client> logger,
            ILocationPostgres locationPostgres)
        {
            _httpClient = httpClient;
            _logger = logger;
            _locationPostgres = locationPostgres;
        }

        public async Task ImportOfficalAccessAddress()
        {

            _logger.LogInformation("Getting postcodes");
            var postCodes = await GetPostCodes();
            _logger.LogInformation("Getting roads");
            var roads = await GetRoads();

            var accessAddressUrl = "https://api.dataforsyningen.dk/replikering/udtraek?entitet=adgangsadresse";
            var count = 0;

            using var response = await _httpClient.GetAsync(accessAddressUrl, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var addresses = new List<DawaOfficalAccessAddress>();
            var serializer = new JsonSerializer();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var address = serializer.Deserialize<DawaOfficalAccessAddress>(reader);

                    if (postCodes.TryGetValue(address.PostDistrictCode, out var postDistrictName))
                    {
                        address.PostDistrictName = postDistrictName;
                    }

                    if (roads.TryGetValue(address.RoadExternalId, out var roadName))
                    {
                        address.RoadName = roadName;
                    }

                    addresses.Add(address);

                    if (addresses.Count == 10000)
                    {
                        count += 10000;
                        _logger.LogInformation($"Imported {nameof(DawaOfficalAccessAddress)}: {count}");
                        await _locationPostgres.InsertOfficalAccessAddresses(addresses);

                        addresses.Clear();
                    }
                }
            }

            await _locationPostgres.InsertOfficalAccessAddresses(addresses);
        }

        private async Task<Dictionary<string, string>> GetRoads()
        {
            var serializer = new JsonSerializer();
            var postNumberUrl = "https://api.dataforsyningen.dk/replikering/udtraek?entitet=navngivenvej";
            var postNumberResponse = await _httpClient.GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead);

            var stream = await postNumberResponse.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var result = serializer.Deserialize<List<Road>>(reader);

            return result.ToDictionary(x => x.Id, x => x.Name);
        }

        private async Task<Dictionary<string, string>> GetPostCodes()
        {
            var serializer = new JsonSerializer();
            var postNumberUrl = "https://api.dataforsyningen.dk/replikering/udtraek?entitet=postnummer";
            var postNumberResponse = await _httpClient.GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead);

            var stream = await postNumberResponse.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var result = serializer.Deserialize<List<PostCode>>(reader);

            return result.ToDictionary(x => x.Number, x => x.Name);
        }
    }
}
