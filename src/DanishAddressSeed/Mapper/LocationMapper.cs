using System;
using DanishAddressSeed.Dawa;
using DanishAddressSeed.Location;

namespace DanishAddressSeed.Mapper
{
    internal class LocationMapper : ILocationMapper
    {
        public OfficialAccessAddress Map(DawaOfficalAccessAddress dawaAddress,
                                         string postDistrictName,
                                         string roadName,
                                         bool deleted = false)
        {
            return new OfficialAccessAddress
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
                Deleted = deleted
            };
        }

        public OfficialUnitAddress Map(DawaOfficalUnitAddress dawaAddress, bool deleted = false)
        {
            return new OfficialUnitAddress
            {
                Created = dawaAddress.Created,
                FloorName = dawaAddress.FloorName,
                Id = Guid.NewGuid(),
                Status = GetStatusStringRepresentation(dawaAddress.Status),
                SuitName = dawaAddress.SuitName,
                UnitAddressExternalId = dawaAddress.UnitAddressExternalId,
                Updated = dawaAddress.Updated,
                AccessAddressExternalId = dawaAddress.AccessAddressExternalId,
                Deleted = deleted
            };
        }

        public TypesenseOfficalAccessAddress Map(OfficialAccessAddress address)
        {
            return new TypesenseOfficalAccessAddress
            {
                EastCoordinate = address.EastCoordinate?.ToString() ?? "",
                NorthCoordinate = address.NorthCoordinate?.ToString() ?? "",
                Id = address.Id.ToString(),
                PostDistrictCode = address.PostDistrictCode,
                PostDistrictName = address.PostDistrictName,
                RoadNameHouseNumber = $"{address.RoadName} {address.HouseNumber}",
                TownName = address.TownName
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
