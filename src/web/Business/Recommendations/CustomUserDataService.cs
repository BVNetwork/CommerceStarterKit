using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using EPiServer.ConnectForCampaign.Core.Implementation;
using EPiServer.Framework;
using EPiServer.Tracking.Commerce;
using Mediachase.Commerce.Customers;

namespace OxxCommerceStarterKit.Web.Business.Recommendations
{
    public class CustomUserDataService : IUserDataService
    {
        private readonly IVisitorIdentifyProvider _visitorIdentifyProvider;

        public CustomUserDataService(IVisitorIdentifyProvider visitorIdentifyProvider)
        {
            _visitorIdentifyProvider = visitorIdentifyProvider;
        }

        private static string EnsureEmailAddress(string userNameOrEmail, string hostName)
        {
            if (string.IsNullOrEmpty(userNameOrEmail))
            {
                return null;
            }
            if (Validator.EmailRegex.IsMatch(userNameOrEmail))
            {
                return userNameOrEmail;
            }
            return string.Concat(userNameOrEmail, "@", hostName, ".invalid");
        }

        /// <summary>
        /// Gets the additional information of the currently logged in user.
        /// </summary>
        /// <returns>The additional information of the currently logged in user stored as a dictionary.</returns>
        public IDictionary<string, string> GetAdditionalInformation()
        {
            CustomerAddress customerAddress;
            CustomerContext current = CustomerContext.Current;
            if (current != null)
            {
                CustomerContact currentContact = current.CurrentContact;
                if (currentContact != null)
                {
                    customerAddress = currentContact.ContactAddresses.FirstOrDefault<CustomerAddress>();
                }
                else
                {
                    customerAddress = null;
                }
            }
            else
            {
                customerAddress = null;
            }
            CustomerAddress customerAddress1 = customerAddress;
            if (customerAddress1 == null)
            {
                return null;
            }
            return new Dictionary<string, string>()
            {
                { "StreetAddress", string.Concat(customerAddress1.Line1, " ", customerAddress1.Line2) },
                { "Phone", customerAddress1.DaytimePhoneNumber },
                { "City", customerAddress1.City },
                { "State", customerAddress1.State },
                { "ZipCode", customerAddress1.PostalCode }
            };
        }

        /// <summary>
        /// Gets the email address of the currently logged in user. 
        /// </summary>
        /// <param name="httpContext">The http context.</param>
        /// <returns>The email address or null if no user logged in.</returns>
        public string GetUserEmail(HttpContextBase httpContext)
        {

            string email;
            string name;
            string host;
            CustomerContext current = CustomerContext.Current;
            if (current != null)
            {
                CustomerContact currentContact = current.CurrentContact;
                if (currentContact != null)
                {
                    email = currentContact.Email;
                }
                else
                {
                    email = null;
                }
            }
            else
            {
                email = null;
            }
            string str = email;
            if (string.IsNullOrEmpty(str))
            {
                if (httpContext != null)
                {
                    IPrincipal user = httpContext.User;
                    if (user != null)
                    {
                        IIdentity identity = user.Identity;
                        if (identity != null)
                        {
                            name = identity.Name;
                        }
                        else
                        {
                            name = null;
                        }
                    }
                    else
                    {
                        name = null;
                    }
                }
                else
                {
                    name = null;
                }
                if (httpContext != null)
                {
                    HttpRequestBase request = httpContext.Request;
                    if (request != null)
                    {
                        Uri url = request.Url;
                        if (url != null)
                        {
                            host = url.Host;
                        }
                        else
                        {
                            host = null;
                        }
                    }
                    else
                    {
                        host = null;
                    }
                }
                else
                {
                    host = null;
                }
                str = EnsureEmailAddress(name, host);
            }
            
            if(string.IsNullOrWhiteSpace(str))
                str = _visitorIdentifyProvider.GetVisitorIdentifier();

            return str;
        }

        /// <summary>
        /// Gets the name of the currently logged in user. 
        /// </summary>
        /// <param name="httpContext">The http context.</param>
        /// <returns>The name of the currently logged in user null if no user logged in or the name is unavailable.</returns>
        public string GetUserName(HttpContextBase httpContext)
        {
            CustomerContext current = CustomerContext.Current;
            if (current == null)
            {
                return null;
            }
            CustomerContact currentContact = current.CurrentContact;
            if (currentContact != null)
            {
                return currentContact.FullName;
            }
            return null;
        }
    }
}