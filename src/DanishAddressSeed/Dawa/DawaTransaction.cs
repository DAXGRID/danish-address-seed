using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal record DawaTransaction
    {
        [JsonProperty("txid")]
        public string TxId { get; init; }
    }
}
