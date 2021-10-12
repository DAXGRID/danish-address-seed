using System;

namespace DanishAddressSeed.Location
{
    internal record OfficialAccessAddress
    {
        public Guid Id { get; init; }
        public DateTime Created { get; init; }
        public DateTime Updated { get; init; }
        public string MunicipalCode { get; init; }
        public string Status { get; init; }
        public string RoadCode { get; init; }
        public string HouseNumber { get; init; }
        public string PostDistrictCode { get; init; }
        public string PostDistrictName { get; init; }
        public double? EastCoordinate { get; init; }
        public double? NorthCoordinate { get; init; }
        public DateTime LocationUpdated { get; init; }
        public string AccessAdddressExternalId { get; init; }
        public string TownName { get; init; }
        public string PlotExternalId { get; init; }
        public string RoadExternalId { get; init; }
        public string RoadName { get; init; }
        public bool Deleted { get; init; }
    }
}
