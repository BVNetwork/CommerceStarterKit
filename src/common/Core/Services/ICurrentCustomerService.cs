using System;
using Mediachase.Commerce.Customers;

namespace OxxCommerceStarterKit.Core.Services
{
    public interface ICurrentCustomerService
    {
        string GetCurrentUserId();

        /// <summary>
        /// Returns Commerce contact id for logged on users, and profile guid for anonymous users
        /// </summary>
        /// <returns></returns>
        Guid GetCurrentUserGuid();

        CustomerContact GetContactById(Guid contactId);
    }
}