using System;
using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Customers;
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
        public virtual Guid GetCurrentUserGuid()
        {
            return EPiServer.Security.PrincipalInfo.CurrentPrincipal.GetContactId();
        }

        public virtual string GetCurrentUserId()
        {
            return GetCurrentUserGuid().ToString();
        }

        public virtual CustomerContact GetContactById(Guid contactId)
        {
            return CustomerContext.Current.GetContactById(contactId);
        }


    }
}
