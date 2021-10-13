using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace DanishAddressSeed.Dawa
{
    internal enum Status
    {
        [Description("Active")]
        Active = 1,
        [Description("Canceled")]
        Canceled = 2,
        [Description("Pending")]
        Pending = 3,
        [Description("Discontinued")]
        Discontinued = 4
    }

    internal record DawaOfficalAccessAddress
    {
        [JsonProperty("oprettet")]
        public DateTime Created { get; init; }
        [JsonProperty("ændret")]
        public DateTime Updated { get; init; }
        [JsonProperty("kommunekode")]
        public string MunicipalCode { get; init; }
        [JsonProperty("status")]
        public Status Status { get; init; }
        [JsonProperty("vejkode")]
        public string RoadCode { get; init; }
        [JsonProperty("husnr")]
        public string HouseNumber { get; init; }
        [JsonProperty("postnr")]
        public string PostDistrictCode { get; init; }
        [JsonProperty("etrs89koordinat_øst")]
        public double? EastCoordinate { get; init; }
        [JsonProperty("etrs89koordinat_nord")]
        public double? NorthCoordinate { get; init; }
        [JsonProperty("adressepunktændringsdato")]
        public DateTime? LocationUpdated { get; init; }
        [JsonProperty("id")]
        public string AccessAdddressExternalId { get; init; }
        [JsonProperty("supplerendebynavn")]
        public string TownName { get; init; }
        [JsonProperty("matrikelnr")]
        public string PlotExternalId { get; init; }
        [JsonProperty("navngivenvej_id")]
        public string RoadExternalId { get; init; }
    }
}
