using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DanishAddressSeed.Location
{
    internal interface ILocationPostgres
    {
        Task InsertOfficalAccessAddresses(List<OfficialAccessAddress> addresses);
        Task UpdateOfficalAccessAddress(OfficialAccessAddress address);
        Task InsertOfficialUnitAddresses(List<OfficialUnitAddress> addresses);
        Task UpdateOfficialUnitAddress(OfficialUnitAddress address);
        Task<Guid> GetAccessAddressOnExternalId(string externalId);
        Task InsertTransactionHistory(string tId, DateTime tTimestamp);
        Task<string> GetLatestTransactionHistory();
    }
}
