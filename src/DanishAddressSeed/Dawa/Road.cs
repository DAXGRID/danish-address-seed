using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal class Road
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("navn")]
        public string Name { get; set; }
    }
}
