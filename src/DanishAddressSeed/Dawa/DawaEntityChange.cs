using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal class DawaEntityChange<T>
    {
        [JsonProperty("txtid")]
        public string TxtId { get; set; }
        [JsonProperty("operation")]
        public string Operation { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
