using System.Collections.Generic;
using System.Threading.Tasks;
using DanishAddressSeed.Dawa;

namespace DanishAddressSeed.Location
{
    internal interface ILocationPostgres
    {
        Task InsertOfficalAccessAddresses(List<OfficalAccessAddress> addresses);
    }
}
