using System.Web.Http;
using System.Web.Mvc;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using InitializationModule = EPiServer.Web.InitializationModule;

namespace OxxCommerceStarterKit.Web.Business
{
    [InitializableModule]
    [ModuleDependency(typeof(InitializationModule))]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
      
            DependencyResolver.SetResolver(new StructureMapDependencyResolver(context.StructureMap()));
            GlobalConfiguration.Configuration.DependencyResolver = new StructureMapDependencyResolver(context.StructureMap());
        }


        public void Initialize(InitializationEngine context)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }
}
