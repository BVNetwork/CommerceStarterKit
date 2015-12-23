using System.Linq;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Interfaces;
using StructureMap;

namespace OxxCommerceStarterKit.Sannsyn
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class ServiceRegistration : IConfigurableModule
    {
        public void Initialize(InitializationEngine context)
        {
            
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Container.Configure(ConfigureContainer);
        }

        private void ConfigureContainer(ConfigurationExpression container)
        {
            container.For<IRecommendedProductsService>().Use<SannsynRecommendedProductsService>();
        }
    }
}