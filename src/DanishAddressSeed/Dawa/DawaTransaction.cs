using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal class DawaTransaction
    {
        [JsonProperty("txid")]
        public string TxId { get; set; }
    }
}
