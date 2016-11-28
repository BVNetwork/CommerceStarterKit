using System;
using EPiServer.ServiceLocation;

namespace OxxCommerceStarterKit.Core.Facades
{
    [ServiceConfiguration(typeof(AppContextFacade), Lifecycle = ServiceInstanceScope.Singleton)]
    public class AppContextFacade
    {
        public virtual Guid ApplicationId
        {
            get { return Mediachase.Commerce.Core.AppContext.Current.ApplicationId; }
        }
    }
}