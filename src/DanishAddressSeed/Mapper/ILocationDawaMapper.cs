using DanishAddressSeed.Dawa;
using DanishAddressSeed.Location;

namespace DanishAddressSeed.Mapper
{
    internal interface ILocationDawaMapper
    {
        OfficialAccessAddress Map(DawaOfficalAccessAddress dawaAddress,
                                 string postDistrictName,
                                 string roadName,
                                 bool deleted = false);
        OfficialUnitAddress Map(DawaOfficalUnitAddress dawaAddress, bool deleted = false);
    }
}
