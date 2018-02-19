using System.Web.Http;
using System.Web.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Personalization.Commerce.CatalogFeed;
using EPiServer.ServiceLocation;

namespace OxxCommerceStarterKit.Web.Business.Recommendations
{
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class RecommendationInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {

            //// Add catalog feed route
            RouteTable.Routes.MapHttpRoute(
                "episerverapi",
                "episerverapi/getcatalogfeed/{id}",
                new
                {
                    controller = "CatalogFeedExport",
                    action = "GetFeed",
                    id = RouteParameter.Optional
                });


            var catalogFeedSettings = ServiceLocator.Current.GetInstance<CatalogFeedSettings>();
            catalogFeedSettings.DescriptionPropertyName = "Description";
            catalogFeedSettings.AssetGroupName = "Default";
            catalogFeedSettings.ExcludedAttributes = new[] { "Overview" };
        }

        public void Uninitialize(InitializationEngine context)
        {

        }
    }
}