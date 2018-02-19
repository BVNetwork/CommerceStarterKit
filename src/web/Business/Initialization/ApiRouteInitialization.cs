using System.Web.Http;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace OxxCommerceStarterKit.Web.Business.Initialization
{
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class ApiRouteInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                "Api", // Route name 
                "api/{controller}/{action}/{id}", // URL with parameters 
                new { id = RouteParameter.Optional, action = "Get" } // Parameter defaults 
            );

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                "LanguageAwareApi", // Route name 
                "{language}/api/{controller}/{action}/{id}", // URL with parameters 
                new { id = RouteParameter.Optional } // Parameter defaults
            );
        }

        public void Uninitialize(InitializationEngine context)
        {
           
        }
    }
}