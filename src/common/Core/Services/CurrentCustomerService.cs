using EPiServer.ServiceLocation;
using Mediachase.Commerce.Security;

namespace OxxCommerceStarterKit.Core.Services
{
    [ServiceConfiguration(typeof(ICurrentCustomerService))]
    public class CurrentCustomerService : ICurrentCustomerService
    {
        /// <summary>
        /// Returns Commerce contact id for logged on users, and profile guid for anonymous users
        /// </summary>
        /// <returns></returns>
        public string GetCurrentUserId()
        {
            // 
            var userId = EPiServer.Security.PrincipalInfo.CurrentPrincipal.GetContactId().ToString();
            return userId;
        }

    }
}
