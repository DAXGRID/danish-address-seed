using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DanishAddressSeed.Location
{
    internal interface ILocationPostgres
    {
        Task InsertOfficalAccessAddresses(List<OfficalAccessAddress> addresses);
        Task UpdateOfficalAccessAddress(OfficalAccessAddress address);
        Task InsertOfficialUnitAddresses(List<OfficalUnitAddress> addresses);
        Task UpdateOfficialUnitAddress(OfficalUnitAddress address);
        Task InsertTransactionHistory(string tId, DateTime tTimestamp);
        Task<string> GetLatestTransactionHistory();
    }
}
