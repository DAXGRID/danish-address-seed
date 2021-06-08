using System;
using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal record DawaOfficalUnitAddress
    {
        [JsonProperty("adgangsadresseid")]
        public string AccessAddressExternalId { get; set; }
        [JsonProperty("status")]
        public Status Status { get; set; }
        [JsonProperty("etage")]
        public string FloorName { get; set; }
        [JsonProperty("dør")]
        public string SuitName { get; set; }
        [JsonProperty("id")]
        public string UnitAddressExternalId { get; set; }
        [JsonProperty("oprettet")]
        public DateTime Created { get; set; }
        [JsonProperty("ændret")]
        public DateTime Updated { get; set; }
    }
}
