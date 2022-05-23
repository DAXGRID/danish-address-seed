using DanishAddressSeed.Location;
using DanishAddressSeed.Mapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DanishAddressSeed.Dawa
{
    internal sealed class DawaClient : IDawaClient
    {
        private const string _dawaBasePath = "https://api.dataforsyningen.dk/replikering";
        private readonly HttpClient _httpClient;
        private readonly ILogger<DawaClient> _logger;
        private readonly ILocationMapper _locationDawaMapper;

        public DawaClient(
            HttpClient httpClient,
            ILogger<DawaClient> logger,
            ILocationMapper locationDawaMapper)
        {
            _httpClient = httpClient;
            _logger = logger;
            _locationDawaMapper = locationDawaMapper;
        }

        public async Task<string> GetTransactionId()
        {
            var transactionUrl = $"{_dawaBasePath}/senestetransaktion";
            using var response = await _httpClient.GetAsync(
                transactionUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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

        public async IAsyncEnumerable<OfficialAccessAddress> RetrieveAllOfficialAccessAddresses(string tId)
        {
            var postCodesTask = GetPostCodes(tId).ConfigureAwait(false);
            var roadsTask = GetRoads(tId).ConfigureAwait(false);
            var postCodes = await postCodesTask;
            var roads = await roadsTask;

            var accessAddressUrl = $"{_dawaBasePath}/udtraek?entitet=adgangsadresse&ndjson&txid={tId}";
            using var response = await _httpClient.GetAsync(
               accessAddressUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader)
            {
                SupportMultipleContent = true
            };

            var serializer = new JsonSerializer();
            while (await reader.ReadAsync().ConfigureAwait(false))
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

                    yield return _locationDawaMapper.Map(address, postDistrictName, roadName);
                }
            }
        }

        public async IAsyncEnumerable<OfficialUnitAddress> RetrieveAllOfficalUnitAddresses(string tId)
        {
            var unitAddressUrl = $"{_dawaBasePath}/udtraek?entitet=adresse&ndjson&txid={tId}";

            using var response = await _httpClient.GetAsync(
                unitAddressUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader)
            {
                SupportMultipleContent = true
            };

            var serializer = new JsonSerializer();
            while (await reader.ReadAsync().ConfigureAwait(false))
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

                    yield return _locationDawaMapper.Map(unitAddress);
                }
            }
        }

        public async IAsyncEnumerable<EntityChange<OfficialAccessAddress>> RetrieveChangesOfficalAccessAddress(string fromTransId, string toTransId)
        {
            var postCodesTask = GetPostCodes(toTransId);
            var roadsTask = GetRoads(toTransId);
            var postCodes = await postCodesTask;
            var roads = await roadsTask;

            var url = @$"{_dawaBasePath}/haendelser?entitet=adgangsadresse&txidfra={fromTransId}&txidtil={toTransId}";
            using var accessAddressResponse = await _httpClient
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            using var stream = await accessAddressResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader)
            {
                SupportMultipleContent = true
            };

            var serializer = new JsonSerializer();
            var dawaChangeEvents =
                serializer.Deserialize<List<DawaEntityChange<DawaOfficalAccessAddress>>>(reader);

            foreach (var changeEvent in dawaChangeEvents)
            {
                postCodes.TryGetValue(changeEvent.Data.PostDistrictCode, out var postDistrictName);
                roads.TryGetValue(changeEvent.Data.RoadExternalId, out var roadName);

                var mapped = changeEvent.Operation == "delete"
                    ? _locationDawaMapper.Map(changeEvent.Data, postDistrictName, roadName, true)
                    : _locationDawaMapper.Map(changeEvent.Data, postDistrictName, roadName);

                yield return new EntityChange<OfficialAccessAddress>
                {
                    Operation = changeEvent.Operation,
                    Data = mapped
                };
            }
        }

        public async IAsyncEnumerable<EntityChange<OfficialUnitAddress>> RetrieveChangesOfficialUnitAddress(
            string fromTransId, string toTransId)
        {
            var url = @$"{_dawaBasePath}/haendelser?entitet=adresse&txidfra={fromTransId}&txidtil={toTransId}";
            using var response = await _httpClient
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var serializer = new JsonSerializer();
            var dawaChangeEvents =
                serializer.Deserialize<List<DawaEntityChange<DawaOfficalUnitAddress>>>(reader);

            foreach (var changeEvent in dawaChangeEvents)
            {
                var mapped = changeEvent.Operation == "delete"
                    ? _locationDawaMapper.Map(changeEvent.Data, true)
                    : _locationDawaMapper.Map(changeEvent.Data);

                yield return new EntityChange<OfficialUnitAddress>
                {
                    Operation = changeEvent.Operation,
                    Data = mapped
                };
            }
        }

        private async Task<Dictionary<string, string>> GetRoads(string tId)
        {
            var postNumberUrl = $"{_dawaBasePath}/udtraek?entitet=navngivenvej&txid={tId}";
            using var response = await _httpClient
                .GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var serializer = new JsonSerializer();
            return serializer.Deserialize<List<DawaRoad>>(reader).ToDictionary(x => x.Id, x => x.Name);
        }

        private async Task<Dictionary<string, string>> GetPostCodes(string tId)
        {
            var serializer = new JsonSerializer();
            var postNumberUrl = $"{_dawaBasePath}/udtraek?entitet=postnummer&txid={tId}";

            using var response = await _httpClient
                .GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            return serializer
                .Deserialize<List<DawaPostCode>>(reader)
                .ToDictionary(x => x.Number, x => x.Name);
        }
    }
}
