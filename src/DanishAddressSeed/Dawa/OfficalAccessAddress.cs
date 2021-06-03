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

    internal class DawaOfficalAccessAddress
    {
        [JsonProperty("oprettet")]
        public DateTime Created { get; set; }
        [JsonProperty("ændret")]
        public DateTime Updated { get; set; }
        [JsonProperty("kommunekode")]
        public string MunicipalCode { get; set; }
        [JsonProperty("status")]
        public Status Status { get; set; }
        [JsonProperty("vejkode")]
        public string RoadCode { get; set; }
        [JsonProperty("husnr")]
        public string HouseNumber { get; set; }
        [JsonProperty("postnr")]
        public string PostDistrictCode { get; set; }
        public string PostDistrictName { get; set; }
        [JsonProperty("etrs89koordinat_øst")]
        public double EastCoordinate { get; set; }
        [JsonProperty("etrs89koordinat_nord")]
        public double NorthCoordinate { get; set; }
        [JsonProperty("adressepunktændringsdato")]
        public DateTime LocationUpdated { get; set; }
        [JsonProperty("id")]
        public string AccessAdddressExternalId { get; set; }
        [JsonProperty("supplerendebynavn")]
        public string TownName { get; set; }
        [JsonProperty("matrikelnr")]
        public string PlotExternalId { get; set; }
        [JsonProperty("navngivenvej_id")]
        public string RoadExternalId { get; set; }
        public string RoadName { get; set; }

        public string GetStatusStringRepresentation()
        {
            switch (Status)
            {
                case Status.Active:
                    return nameof(Status.Active);
                case Status.Canceled:
                    return nameof(Status.Canceled);
                case Status.Discontinued:
                    return nameof(Status.Discontinued);
                case Status.Pending:
                    return nameof(Status.Pending);
                default:
                    throw new Exception($"Status is not valid with enum-id: '{Status}'");
            }
        }
    }
}
