using System;
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
        private const string _dawaBasePath = "https://api.dataforsyningen.dk/replikering";
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

        public async Task<string> GetTransactionId()
        {
            var transactionUrl = "${_dawaBasePath}/senestetransaktion";

            using var response = await _httpClient
                          .GetAsync(transactionUrl, HttpCompletionOption.ResponseHeadersRead);
            var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var serializer = new JsonSerializer();
            var result = serializer.Deserialize<DawaTransaction>(reader);

            if (string.IsNullOrEmpty(result.TxId))
            {
                throw new Exception("txid cannot be empty");
            }

            return result.TxId;
        }

        public async Task BulkOfficalAccessAddress(string tId)
        {
            var postCodesTask = GetPostCodes(tId);
            var roadsTask = GetRoads(tId);

            var postCodes = await postCodesTask;
            var roads = await roadsTask;

            var accessAddressUrl = $"{_dawaBasePath}/udtraek?entitet=adgangsadresse&ndjson&txid={tId}";
            var count = 0;

            using var response = await _httpClient.GetAsync(accessAddressUrl, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader)
            {
                SupportMultipleContent = true
            };

            var addresses = new List<OfficalAccessAddress>();
            var serializer = new JsonSerializer();
            while (await reader.ReadAsync())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    DawaOfficalAccessAddress address = null;
                    try
                    {
                        address = serializer.Deserialize<DawaOfficalAccessAddress>(reader);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        continue;
                    }

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

        public async Task BulkImportOfficalUnitAddress(string tId)
        {
            var unitAddressUrl = $"{_dawaBasePath}/udtraek?entitet=adresse&ndjson&txid={tId}";
            var count = 0;

            using var response = await _httpClient.GetAsync(unitAddressUrl, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader)
            {
                SupportMultipleContent = true
            };

            var addresses = new List<OfficalUnitAddress>();
            var serializer = new JsonSerializer();
            while (await reader.ReadAsync())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    DawaOfficalUnitAddress unitAddress = null;
                    try
                    {
                        unitAddress = serializer.Deserialize<DawaOfficalUnitAddress>(reader);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            $"JSON deserialize failed with error ${ex.Message} and content {reader.Value}");
                        continue;
                    }

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

        private async Task<Dictionary<string, string>> GetRoads(string tId)
        {
            var serializer = new JsonSerializer();
            var postNumberUrl = $"{_dawaBasePath}/udtraek?entitet=navngivenvej&txid={tId}";
            var postNumberResponse = await _httpClient
                .GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead);

            var stream = await postNumberResponse.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var result = serializer.Deserialize<List<DawaRoad>>(reader);

            return result.ToDictionary(x => x.Id, x => x.Name);
        }

        private async Task<Dictionary<string, string>> GetPostCodes(string tId)
        {
            var serializer = new JsonSerializer();
            var postNumberUrl = $"{_dawaBasePath}/udtraek?entitet=postnummer&txid={tId}";
            var postNumberResponse = await _httpClient
                .GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead);

            var stream = await postNumberResponse.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var result = serializer.Deserialize<List<DawaPostCode>>(reader);

            return result.ToDictionary(x => x.Number, x => x.Name);
        }
    }
}
