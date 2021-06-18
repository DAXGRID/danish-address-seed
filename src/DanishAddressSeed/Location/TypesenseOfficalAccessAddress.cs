namespace DanishAddressSeed.Location
{
    internal record TypesenseOfficalAccessAddress
    {
        public string Id { get; init; }
        public string PostDistrictCode { get; init; }
        public string PostDistrictName { get; init; }
        public string EastCoordinate { get; init; }
        public string NorthCoordinate { get; init; }
        public string TownName { get; init; }
        public string RoadNameHouseNumber { get; init; }
    }
}
