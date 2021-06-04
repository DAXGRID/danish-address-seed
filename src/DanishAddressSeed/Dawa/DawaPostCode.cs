using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal class DawaPostCode
    {
        [JsonProperty("nr")]
        public string Number { get; set; }
        [JsonProperty("navn")]
        public string Name { get; set; }
    }
}
