using System.Security.Principal;
using EPiServer.Framework.Web.Resources;
using EPiServer.Security;
using Mediachase.Commerce.Customers;

namespace OxxCommerceStarterKit.Web.Business.ClientResources
{

    [ClientResourceRegistrator]
    public class ZopimChatClientResourceRegister : IClientResourceRegistrator
    {

        public void RegisterResources(IRequiredClientResourceList requiredResources)
        {
            IPrincipal currentPrincipal = PrincipalInfo.CurrentPrincipal;
            if (currentPrincipal != null && currentPrincipal.Identity.IsAuthenticated)
            {
                var contact = CustomerContext.Current.CurrentContact;
                if (contact != null)
                {
                    string script = @"
if (typeof $zopim !== 'undefined') {
   $zopim(function(){
           $zopim.livechat.setEmail('" + contact.Email + @"');
           $zopim.livechat.setName('" + contact.FirstName + " " + contact.LastName + @"');
    });
}
";
                    requiredResources.RequireScriptInline(script).AtFooter();
                }
            }
        }
    }
}