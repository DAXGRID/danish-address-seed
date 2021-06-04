using System.Threading.Tasks;

namespace DanishAddressSeed.Dawa
{
    internal interface IClient
    {
        Task ImportOfficalAccessAddress();
        Task ImportOfficalUnitAddress();
    }
}
