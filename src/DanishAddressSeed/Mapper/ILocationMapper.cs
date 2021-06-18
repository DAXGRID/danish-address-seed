using DanishAddressSeed.Dawa;
using DanishAddressSeed.Location;

namespace DanishAddressSeed.Mapper
{
    internal interface ILocationMapper
    {
        OfficialAccessAddress Map(DawaOfficalAccessAddress dawaAddress,
                                 string postDistrictName,
                                 string roadName,
                                 bool deleted = false);
        OfficialUnitAddress Map(DawaOfficalUnitAddress dawaAddress, bool deleted = false);
        TypesenseOfficalAccessAddress Map(OfficialAccessAddress address);
    }
}
