using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal record DawaEntityChange<T>
    {
        [JsonProperty("txtid")]
        public string TxtId { get; init; }
        [JsonProperty("operation")]
        public string Operation { get; init; }
        [JsonProperty("data")]
        public T Data { get; init; }
    }
}
