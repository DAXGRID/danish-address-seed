using System;
using DanishAddressSeed.Dawa;
using DanishAddressSeed.Location;

namespace DanishAddressSeed.Mapper
{
    internal class LocationDawaMapper : ILocationDawaMapper
    {
        public OfficalAccessAddress Map(DawaOfficalAccessAddress dawaAddress,
                                        string postDistrictName,
                                        string roadName, bool deleted = false)
        {
            return new OfficalAccessAddress
            {
                AccessAdddressExternalId = dawaAddress.AccessAdddressExternalId,
                Created = dawaAddress.Created,
                EastCoordinate = dawaAddress.EastCoordinate,
                NorthCoordinate = dawaAddress.NorthCoordinate,
                HouseNumber = dawaAddress.HouseNumber,
                Id = Guid.NewGuid(),
                LocationUpdated = dawaAddress.LocationUpdated,
                MunicipalCode = dawaAddress.MunicipalCode,
                PlotExternalId = dawaAddress.PlotExternalId,
                PostDistrictCode = dawaAddress.PostDistrictCode,
                PostDistrictName = postDistrictName,
                RoadCode = dawaAddress.RoadCode,
                RoadExternalId = dawaAddress.RoadExternalId,
                RoadName = roadName,
                Status = GetStatusStringRepresentation(dawaAddress.Status),
                TownName = dawaAddress.TownName,
                Updated = dawaAddress.Updated,
                Deleted = false
            };
        }

        public OfficalUnitAddress Map(DawaOfficalUnitAddress dawaAddress, bool deleted = false)
        {
            return new OfficalUnitAddress
            {
                Created = dawaAddress.Created,
                FloorName = dawaAddress.FloorName,
                Id = Guid.NewGuid(),
                Status = GetStatusStringRepresentation(dawaAddress.Status),
                SuitName = dawaAddress.SuitName,
                UnitAddressExternalId = dawaAddress.UnitAddressExternalId,
                Updated = dawaAddress.Updated,
                AccessAddressExternalId = dawaAddress.AccessAddressExternalId,
                Deleted = false
            };
        }

        private string GetStatusStringRepresentation(Status status)
        {
            switch (status)
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
                    throw new Exception($"Status is not valid with enum-id: '{status}'");
            }
        }
    }
}
