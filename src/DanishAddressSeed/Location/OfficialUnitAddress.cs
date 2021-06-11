using System;

namespace DanishAddressSeed.Location
{
    internal record OfficialUnitAddress
    {
        public Guid Id { get; init; }
        public Guid AccessAddressId { get; init; }
        public string Status { get; init; }
        public string FloorName { get; init; }
        public string SuitName { get; init; }
        public string UnitAddressExternalId { get; init; }
        public DateTime Created { get; init; }
        public DateTime Updated { get; init; }
        public string AccessAddressExternalId { get; init; }
        public bool Deleted { get; init; }
    }
}
