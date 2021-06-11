using System.Collections.Generic;
using System.Threading.Tasks;
using DanishAddressSeed.Location;

namespace DanishAddressSeed.Dawa
{
    internal interface IClient
    {
        IAsyncEnumerable<OfficialAccessAddress> RetrieveAllOfficialAccessAddresses(string tId);
        IAsyncEnumerable<OfficialUnitAddress> RetrieveAllOfficalUnitAddresses(string tId);
        IAsyncEnumerable<EntityChange<OfficialAccessAddress>> RetrieveChangesOfficalAccessAddress(string fromTransId, string toTransId);
        IAsyncEnumerable<EntityChange<OfficialUnitAddress>> RetrieveChangesOfficialUnitAddress(string fromTransId, string toTransId);
        Task<string> GetTransactionId();
    }
}
