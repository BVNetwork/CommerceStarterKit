using System;
using System.Linq;
using System.Web.Security;
using EPiServer.ServiceLocation;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using OxxCommerceStarterKit.Core.Extensions;

namespace OxxCommerceStarterKit.Core.Customers
{
    public class CustomerFactory : ICustomerFactory
    {
        public CustomerContact CreateCustomer(string email, string password, string phone, OrderAddress billingAddress,
            OrderAddress shippingAddress, bool hasPassword, Action<MembershipCreateStatus> userCreationFailed)
        {
            MembershipCreateStatus createStatus;
            var user = Membership.CreateUser(email, password, email, null, null, true, out createStatus);
            switch (createStatus)
            {
                case MembershipCreateStatus.Success:

                    Roles.AddUserToRole(user.UserName, Mediachase.Commerce.Core.AppRoles.EveryoneRole);
                    Roles.AddUserToRole(user.UserName, Mediachase.Commerce.Core.AppRoles.RegisteredRole);

                    var customer = CustomerContext.Current.GetContactForUser(user);
                    customer.FirstName = billingAddress.FirstName;
                    customer.LastName = billingAddress.LastName;
                    customer.FullName = string.Format("{0} {1}", customer.FirstName, customer.LastName);
                    customer.SetPhoneNumber(phone);
                    customer.SetHasPassword(hasPassword);

                    var customerBillingAddress = CustomerAddress.CreateForApplication(Mediachase.Commerce.Core.AppContext.Current.ApplicationId);
                    OrderAddress.CopyOrderAddressToCustomerAddress(billingAddress, customerBillingAddress);
                    customer.AddContactAddress(customerBillingAddress);
                    customer.SaveChanges();
                    customer.PreferredBillingAddressId = customerBillingAddress.AddressId;
                    customerBillingAddress.Name = string.Format("{0}, {1} {2}", customerBillingAddress.Line1,
                        customerBillingAddress.PostalCode, customerBillingAddress.City);
                    CheckCountryCode(customerBillingAddress);
                    BusinessManager.Update(customerBillingAddress);
                    customer.SaveChanges();

                    var customerShippingAddress = CustomerAddress.CreateForApplication(Mediachase.Commerce.Core.AppContext.Current.ApplicationId);
                    OrderAddress.CopyOrderAddressToCustomerAddress(shippingAddress, customerShippingAddress);
                    customer.AddContactAddress(customerShippingAddress);
                    customer.SaveChanges();
                    customer.PreferredShippingAddressId = customerShippingAddress.AddressId;
                    customerShippingAddress.Name = string.Format("{0}, {1} {2}", customerShippingAddress.Line1,
                        customerShippingAddress.PostalCode, customerShippingAddress.City);
                    CheckCountryCode(customerShippingAddress);
                    BusinessManager.Update(customerShippingAddress);
                    customer.SaveChanges();

                    return customer;
                default:
                    userCreationFailed(createStatus);
                    break;

            }
            return null;
        }

        /// <summary>
        /// If the customer has not selected a country, we pick the 
        /// first country for the current market.
        /// </summary>
        /// <param name="address">The address.</param>
        private static void CheckCountryCode(CustomerAddress address)
        {
            if (string.IsNullOrEmpty(address.CountryCode))
            {
                var currentMarket = ServiceLocator.Current.GetInstance<ICurrentMarket>(); // TODO: constructor inject
                var market = currentMarket.GetCurrentMarket();
                if (market != null && market.Countries.Any())
                {
                    address.CountryCode = market.Countries.FirstOrDefault();
                }
            }
        }
    }
}