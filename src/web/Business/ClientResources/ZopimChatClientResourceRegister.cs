using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using EPiServer.Framework.Web.Resources;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Security;

namespace OxxCommerceStarterKit.Web.Business.ClientResources
{
    [ClientResourceRegister]

    public class ZopimChatClientResourceRegister : IClientResourceRegister
    {
        public void RegisterResources(IRequiredClientResourceList requiredResources, HttpContextBase context)
        {
            IPrincipal currentPrincipal = PrincipalInfo.CurrentPrincipal;
            if (currentPrincipal != null && currentPrincipal.Identity.IsAuthenticated)
            {
                var contact = CustomerContext.Current.CurrentContact;
                if (contact != null)
                {
                    string script = @"
   $zopim(function(){
           $zopim.livechat.setEmail('" + contact.Email + @"');
           $zopim.livechat.setName('" + contact.FirstName + " " + contact.LastName + @"');
    });
";
                    requiredResources.RequireScriptInline(script).AtFooter();
                }
            }
        }
    }
}