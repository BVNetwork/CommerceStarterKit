/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Routing;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using EPiServer.Web.Routing.Segments;

using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using OxxCommerceStarterKit.Core;
using OxxCommerceStarterKit.Core.Customers;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Web.Business.ClientTracking;
using OxxCommerceStarterKit.Web.Business.Payment;
using OxxCommerceStarterKit.Web.Controllers;
using OxxCommerceStarterKit.Web.ModelBuilders;
using OxxCommerceStarterKit.Web.ResetPassword;
using OxxCommerceStarterKit.Web.Services;
using OxxCommerceStarterKit.Web.Services.Email;
using OxxCommerceStarterKit.Web.Services.Email.Models;
using Postal;
using EmailService = OxxCommerceStarterKit.Web.Services.Email.EmailService;
using IEmailService = OxxCommerceStarterKit.Core.Email.IEmailService;

namespace OxxCommerceStarterKit.Web.Business.Initialization
{
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class CommerceInitialization : IInitializableModule, IConfigurableModule
    {
        private ILogger _log = LogManager.GetLogger();

        public void Initialize(InitializationEngine context)
        {
            MapCatalogRoute(RouteTable.Routes);

            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            GlobalFilters.Filters.Add(ServiceLocator.Current.GetInstance<PageContextActionFilter>());

            ModelMetadataProviders.Current = new CustomModelMetadataProvider();
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;


            var connectionString = ConfigurationManager.ConnectionStrings["EcfSqlConnection"].ConnectionString;
            DataContext.Current = new DataContext(connectionString);


            var associationTypeRepository = context.Locate.Advanced.GetInstance<GroupDefinitionRepository<AssociationGroupDefinition>>();

            // Note! this had a bug in 8.0.0, fixed in 8.0.1 and later
            associationTypeRepository.Add(new AssociationGroupDefinition() { Name = Constants.AssociationTypes.SameStyle });
            associationTypeRepository.Add(new AssociationGroupDefinition() { Name = Constants.AssociationTypes.Accessory });
            associationTypeRepository.Add(new AssociationGroupDefinition() { Name = Constants.AssociationTypes.CrossSell });
            associationTypeRepository.Add(new AssociationGroupDefinition() { Name = Constants.AssociationTypes.Upsell});
            associationTypeRepository.Add(new AssociationGroupDefinition() { Name = Constants.AssociationTypes.Replacement });
            // Default is the "Goes well with" association
            associationTypeRepository.Add(new AssociationGroupDefinition() { Name = Constants.AssociationTypes.Default });
            associationTypeRepository.Delete(Constants.AssociationTypes.RecommendedProducts);
        }

        /// <summary>
        /// Configure default routing for EPiServer Commerce catalog content
        /// </summary>
        /// <remarks>
        /// TODO: If you want to remove the name of the catalog from the url, set the
        /// Catalog:RemoveCatalogFromUrl appSetting to true
        /// </remarks>
        /// <param name="routes"></param>
        private void MapCatalogRoute(RouteCollection routes)
        {
            string removeCatalogFromUrlSetting = System.Configuration.ConfigurationManager.AppSettings["Catalog:RemoveCatalogFromUrl"];
            bool removeCatalogFromUrl = false;
            bool.TryParse(removeCatalogFromUrlSetting, out removeCatalogFromUrl);
            if (removeCatalogFromUrl == false)
            {
                CatalogRouteHelper.MapDefaultHierarchialRouter(routes, false);

            }
            else
            {
                // This will pick the first catalog, and strip it from all urls (in and out)
                var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
                var referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
                
                var firstCatalog =
                    contentLoader.GetChildren<CatalogContent>(referenceConverter.GetRootLink()).FirstOrDefault();

                var partialRouter2 = new HierarchicalCatalogPartialRouter(
                    () => SiteDefinition.Current.StartPage, firstCatalog, false);

                if (firstCatalog != null)
                {

                    routes.RegisterPartialRouter(partialRouter2);
                }
            }
        }

        public void Preload(string[] parameters)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var structureMap = context.StructureMap();

            // Important configuration. Determines the current market
            // TODO: Verify that you want to resolve the market from the language of the start page.
            //       You can also use the CurrentMarketProfile class to store the value in the session
            structureMap.Configure(c => c.For<ICurrentMarket>().Singleton().Use<CurrentMarketFromStartPage>());

            structureMap.Configure(c => c.For<IResetPasswordService>().Use<ResetPasswordService>());
            structureMap.Configure(c => c.For<IResetPasswordRepository>().Use<ResetPasswordRepository>());
            structureMap.Configure(c => c.For<ICartService>().Use<CartService>());

            structureMap.Configure(c => c.For<IEmailService>().Use<EmailService>());
            structureMap.Configure(c => c.For<IEmailDispatcher>().Use<EmailDispatcher>());
            structureMap.Configure(c => c.For<INotificationSettingsRepository>().Use<NotificationSettingsRepository>());

            // Postal
            structureMap.Configure(c => c.For<Postal.IEmailService>().Use<Postal.EmailService>());
            structureMap.Configure(c => c.For<IEmailViewRenderer>().Use<EmailViewRenderer>());
            structureMap.Configure(c => c.For<IEmailParser>().Use<EmailParser>());

            structureMap.Configure(c => c.For<IReceiptViewModelBuilder>().Singleton().Use<ReceiptViewModelBuilder>());
            structureMap.Configure(c => c.For<IDibsPaymentProcessor>().Singleton().Use<DibsPaymentProcessor>());
            structureMap.Configure(c => c.For<ICustomerFactory>().Singleton().Use<CustomerFactory>());
            structureMap.Configure(c => c.For<ISiteSettingsProvider>().Singleton().Use<SiteConfiguration>());
            structureMap.Configure(c => c.For<IGoogleAnalyticsTracker>().Singleton().Use<GoogleAnalyticsTracker>());
            structureMap.Configure(c => c.For<IIdentityProvider>().Singleton().Use<HttpContextIdentityProvider>());
            structureMap.Configure(c => c.For<IOrderService>().Singleton().Use<OrderService>());
            structureMap.Configure(c => c.For<IPaymentCompleteHandler>().Singleton().Use<PaymentCompleteHandler>());
            structureMap.Configure(c => c.For<IHttpContextProvider>().Singleton().Use<HttpContextProvider>());
            structureMap.Configure(c => c.For<IPostNordClient>().Singleton().Use<PostNordClient>());
            structureMap.Configure(c => c.For<IStockUpdater>().Use<StockUpdater>());

            // Include this price service to get random prices
            // structureMap.Configure(c => c.For<IPriceService>().Singleton().Use<OxxCommerceStarterKit.PriceService.RandomPriceService>());
        }
    }
}
