using System;
using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal record DawaOfficalUnitAddress
    {
        [JsonProperty("adgangsadresseid")]
        public string AccessAddressExternalId { get; init; }
        [JsonProperty("status")]
        public Status Status { get; init; }
        [JsonProperty("etage")]
        public string FloorName { get; init; }
        [JsonProperty("dør")]
        public string SuitName { get; init; }
        [JsonProperty("id")]
        public string UnitAddressExternalId { get; init; }
        [JsonProperty("oprettet")]
        public DateTime Created { get; init; }
        [JsonProperty("ændret")]
        public DateTime Updated { get; init; }
    }
}
