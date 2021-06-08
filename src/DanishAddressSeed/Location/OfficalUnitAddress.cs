using System;

namespace DanishAddressSeed.Location
{
    internal record OfficalUnitAddress
    {
        public Guid Id { get; set; }
        public Guid AccessAddressId { get; set; }
        public string Status { get; set; }
        public string FloorName { get; set; }
        public string SuitName { get; set; }
        public string UnitAddressExternalId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string AccessAddressExternalId { get; set; }
    }
}
