using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal record DawaRoad
    {
        [JsonProperty("id")]
        public string Id { get; init; }
        [JsonProperty("navn")]
        public string Name { get; init; }
    }
}
