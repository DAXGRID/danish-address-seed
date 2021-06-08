using System;

namespace DanishAddressSeed.Location
{
    internal record OfficalAccessAddress
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string MunicipalCode { get; set; }
        public string Status { get; set; }
        public string RoadCode { get; set; }
        public string HouseNumber { get; set; }
        public string PostDistrictCode { get; set; }
        public string PostDistrictName { get; set; }
        public double EastCoordinate { get; set; }
        public double NorthCoordinate { get; set; }
        public DateTime LocationUpdated { get; set; }
        public string AccessAdddressExternalId { get; set; }
        public string TownName { get; set; }
        public string PlotExternalId { get; set; }
        public string RoadExternalId { get; set; }
        public string RoadName { get; set; }
    }
}
