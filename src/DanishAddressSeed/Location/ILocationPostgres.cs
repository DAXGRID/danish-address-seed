using System.Collections.Generic;
using System.Threading.Tasks;

namespace DanishAddressSeed.Location
{
    internal interface ILocationPostgres
    {
        Task InsertOfficalAccessAddresses(List<OfficalAccessAddress> addresses);
        Task InsertOfficialUnitAddresses(List<OfficalUnitAddress> addresses);

    }
}
