using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxxCommerceStarterKit.Web.Services
{
    public interface IEspService
    {
        Task<string> Subscribe(string email, IEnumerable<KeyValuePair<string, string>> parameters);
    }
}