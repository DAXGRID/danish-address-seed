using System;
using System.Text.Json.Serialization;

namespace DanishAddressSeed.Dawa
{
    internal class DawaOfficalAccessAddress
    {
        [JsonPropertyName("oprettet")]
        public DateTime Created { get; set; }
        [JsonPropertyName("ændret")]
        public DateTime Changed { get; set; }
        [JsonPropertyName("kommunekode")]
        public string MunicipalCode { get; set; }
        [JsonPropertyName("vejkode")]
        public string RoadCode { get; set; }
        [JsonPropertyName("husnr")]
        public string HouseNumber { get; set; }
        [JsonPropertyName("postnr")]
        public string PostDistrictcode { get; set; }
        [JsonPropertyName("etrs89koordinat_øst")]
        public double EastCoordinate { get; set; }
        [JsonPropertyName("etrs89koordinat_nord")]
        public double NorthCoordinate { get; set; }
        [JsonPropertyName("adressepunktændringsdato")]
        public DateTime LocationUpdated { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("supplerendebynavn")]
        public object TownName { get; set; }
        [JsonPropertyName("matrikelnr")]
        public string PlotExternalId { get; set; }
        [JsonPropertyName("navngivenvej_id")]
        public string RoadExternalId { get; set; }
    }
}
