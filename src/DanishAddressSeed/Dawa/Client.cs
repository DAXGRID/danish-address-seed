using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal class Client : IClient
    {
        private readonly HttpClient _httpClient;

        public Client(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task ImportOfficalAccessAddress()
        {
            var url = "https://api.dataforsyningen.dk/replikering/udtraek?entitet=adgangsadresse";
            var serializer = new JsonSerializer();
            var count = 0;

            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var addresses = new List<DawaOfficalAccessAddress>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var address = serializer.Deserialize<DawaOfficalAccessAddress>(reader);
                    addresses.Add(address);

                    if (addresses.Count == 10000)
                    {
                        count += 10000;
                        Console.WriteLine(count);
                        addresses.Clear();
                    }
                }
            }
        }
    }
}
