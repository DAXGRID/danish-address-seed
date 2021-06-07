using System.Threading.Tasks;

namespace DanishAddressSeed.Dawa
{
    internal interface IClient
    {
        Task BulkOfficalAccessAddress(string tId);
        Task BulkImportOfficalUnitAddress(string tId);
        Task UpdateOfficalAccessAddress(string fromTransId, string toTransId);
        Task UpdateOfficialUnitAddress(string fromTransId, string toTransId);
        Task<string> GetTransactionId();
    }
}
