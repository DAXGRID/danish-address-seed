using System.Threading.Tasks;

namespace DanishAddressSeed.Dawa
{
    internal interface IClient
    {
        Task BulkOfficalAccessAddress(string tId);
        Task BulkImportOfficalUnitAddress(string tId);
        Task<string> GetTransactionId();
    }
}
