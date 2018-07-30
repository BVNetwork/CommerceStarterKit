/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using EPiServer.Find.Cms;
using EPiServer.Find.Cms.Conventions;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Web.Business.Search;
using OxxCommerceStarterKit.Web.Models.Catalog;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.PageTypes.Payment;
using OxxCommerceStarterKit.Web.Models.PageTypes.System;
using OxxCommerceStarterKit.Web.Models.ViewModels.Email;

namespace OxxCommerceStarterKit.Web.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class IndexingEventInitialization : IInitializableModule
    {
        protected static ILogger _log = LogManager.GetLogger();

        public void Initialize(InitializationEngine context)
        {
            //We do not want to index catalog content types, since we are creating our own objects below
            ContentIndexer.Instance.Conventions.ForInstancesOf<FashionItemContent>().ShouldIndex(x => false);
            ContentIndexer.Instance.Conventions.ForInstancesOf<FashionProductContent>().ShouldIndex(x => false);
            ContentIndexer.Instance.Conventions.ForInstancesOf<DigitalCameraVariationContent>().ShouldIndex(x => false);
            ContentIndexer.Instance.Conventions.ForInstancesOf<GenericProductContent>().ShouldIndex(x => false);
            ContentIndexer.Instance.Conventions.ForInstancesOf<GenericSizeVariationContent>().ShouldIndex(x => false);

			// other page types we do not want in the index
			ContentIndexer.Instance.Conventions.ForInstancesOf<ReceiptPage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<OrdersPage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<HomePage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<CartSimpleModulePage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<WishListPage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<ChangePasswordPage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<PersonalInformationPage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<DibsPaymentPage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<SearchPage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<DefaultPage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<LoginPage>().ShouldIndex(x => false);
			ContentIndexer.Instance.Conventions.ForInstancesOf<CheckoutPage>().ShouldIndex(x => false);
			
            // Others
			ContentIndexer.Instance.Conventions.ForInstancesOf<NotificationSettings>().ShouldIndex(x => false);
			
            // Blocks
            // We want editors to be able to search for blocks
            //ContentIndexer.Instance.Conventions.ForInstancesOf<YouTubeBlock>().ShouldIndex(x => false);
            //ContentIndexer.Instance.Conventions.ForInstancesOf<ViddlerBlock>().ShouldIndex(x => false);
            //ContentIndexer.Instance.Conventions.ForInstancesOf<VimeoBlock>().ShouldIndex(x => false);
            //ContentIndexer.Instance.Conventions.ForInstancesOf<TwoColumnsBlock>().ShouldIndex(x => false);
            //ContentIndexer.Instance.Conventions.ForInstancesOf<SocialMediaLinkBlock>().ShouldIndex(x => false);
            //ContentIndexer.Instance.Conventions.ForInstancesOf<ButtonWithHelpLinkBlock>().ShouldIndex(x => false);
            //ContentIndexer.Instance.Conventions.ForInstancesOf<SliderBlock>().ShouldIndex(x => false);
            //ContentIndexer.Instance.Conventions.ForInstancesOf<OneTwoColumnsBlock>().ShouldIndex(x => false);
            //ContentIndexer.Instance.Conventions.ForInstancesOf<PageListBlock>().ShouldIndex(x => false);
            
            // We want editors to be able to search for images
            //ContentIndexer.Instance.Conventions.ForInstancesOf<ImageFile>().ShouldIndex(x => false);
            //ContentIndexer.Instance.Conventions.ForInstancesOf<GenericFile>().ShouldIndex(x => false);

			// hook up events for indexing
            CatalogContentEventIndexer events = ServiceLocator.Current.GetInstance<CatalogContentEventIndexer>();
            events.EnableEventListeners();
        }

        public void Preload(string[] parameters) { }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }
    }
}
