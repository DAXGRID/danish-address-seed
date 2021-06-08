using System.Threading.Tasks;

namespace DanishAddressSeed.Dawa
{
    internal interface IClient
    {
        Task BulkOfficialAccessAddress(string tId);
        Task BulkImportOfficialUnitAddress(string tId);
        Task UpdateOfficalAccessAddress(string fromTransId, string toTransId);
        Task UpdateOfficialUnitAddress(string fromTransId, string toTransId);
        Task<string> GetTransactionId();
    }
}
