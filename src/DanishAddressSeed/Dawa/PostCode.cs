using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal class PostCode
    {
        [JsonProperty("nr")]
        public string Number { get; set; }
        [JsonProperty("navn")]
        public string Name { get; set; }
    }
}
