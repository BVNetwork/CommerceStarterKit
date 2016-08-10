using System.Linq;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Interfaces;
using Sannsyn.Episerver.Commerce.Configuration;
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
            SannsynConfiguration sannsynConfiguration = ServiceLocator.Current.GetInstance<SannsynConfiguration>();
            if (sannsynConfiguration.ModuleEnabled)
            {
                container.For<IRecommendedProductsService>().Use<SannsynRecommendedProductsService>();
            }
            
        }
    }
}