using DanishAddressSeed.Dawa;
using DanishAddressSeed.Location;

namespace DanishAddressSeed.Mapper
{
    internal interface ILocationDawaMapper
    {
        OfficalAccessAddress Map(DawaOfficalAccessAddress dawaAddress, string postDistrictName, string roadName);
    }
}
