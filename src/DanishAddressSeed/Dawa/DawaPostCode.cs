using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal record DawaPostCode
    {
        [JsonProperty("nr")]
        public string Number { get; init; }
        [JsonProperty("navn")]
        public string Name { get; init; }
    }
}
