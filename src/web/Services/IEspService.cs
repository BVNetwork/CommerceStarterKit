using System.Threading.Tasks;

namespace OxxCommerceStarterKit.Web.Services
{
    public interface IEspService
    {
        Task<string> Subscribe(string email, object values);
        Task<string> SingleOptIn(string email, object values);
        Task<string> SubscribeOrRemove(string email, object values);
        void Remove(string userId);
        string GetNewsletterOptions(string email);
    }
}