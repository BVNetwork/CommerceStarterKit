/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Personalization.Commerce.Tracking;
//using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using Mediachase.Commerce;
using OxxCommerceStarterKit.Core;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Business.Rss;
using OxxCommerceStarterKit.Web.Extensions;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class CommerceControllerBase<T> : ContentController<T> where T : CatalogContentBase
    {
        private static Injected<IContentLoader> _contentLoaderService ;
        private static Injected<InventoryLoader> _inventoryLoaderService;
        private static Injected<ReadOnlyPricingLoader> _readonlyPricingLoaderService;
        private static Injected<ICurrentMarket> _icurrentMarketService;
        private static Injected<ILinksRepository> _linksRepositoryService;
        private static Injected<IDefaultInventoryService> _inventoryService;
        private static Injected<ProductService> _productService;

        protected IContentLoader ContentLoader
        {
            get { return _contentLoaderService.Service; }
        }

        protected InventoryLoader InventoryLoader
        {
            get { return _inventoryLoaderService.Service; }
        }

        protected ReadOnlyPricingLoader PricingLoader
        {
            get { return _readonlyPricingLoaderService.Service; }
        }

        protected ICurrentMarket CurrentMarket
        {
            get { return _icurrentMarketService.Service; }
        }

        protected ILinksRepository LinksRepository
        {
            get { return _linksRepositoryService.Service; }
        }

        protected IDefaultInventoryService InventoryService
        {
            get { return _inventoryService.Service; }
        }

        protected ProductService ProductService
        {
            get { return _productService.Service; }
        }


        protected static ILogger _log = LogManager.GetLogger();

        public void InitializeCatalogViewModel<TViewModel>(TViewModel model)
           where TViewModel : ICatalogViewModel<CatalogContentBase>
        {
            model.ChildCategories = model.ChildCategories ?? GetCatalogChildNodes(model.CatalogContent.ContentLink);
            model.Products = model.Products ?? CreateLazyProductContentViewModels(model.CatalogContent);
            model.Variants = model.Variants ?? CreateLazyVariantContentViewModels(model.CatalogContent);            
        }

        public void InitializeVariationViewModel<TViewModel>(TViewModel model)
            where TViewModel : IVariationViewModel<VariationContent>
        {
            model.Inventory = model.Inventory ?? GetInventory(model.CatalogContent, Constants.Warehouse.DefaultWarehouseCode);
            model.Price = model.Price ?? GetPrice(model.CatalogContent);
            model.ParentEntry = model.CatalogContent.GetParent();
            model.ContentWithAssets = model.CatalogContent.CommerceMediaCollection.Any()
                ? model.CatalogContent
                : model.ParentEntry;            
        }

        protected Lazy<IEnumerable<NodeContent>> GetCatalogChildNodes(ContentReference contentLink)
        {
            return new Lazy<IEnumerable<NodeContent>>(() =>
                ContentLoader.GetChildren<NodeContent>(contentLink)
                .ToList());
        }

        protected ICatalogViewModel<CatalogContentBase> CreateFashionCatalogViewModel(CatalogContentBase catalogContent)
        {
            var activator = new Activator<ICatalogViewModel<CatalogContentBase>>();
            var model = activator.Activate(typeof(CatalogContentViewModel<>), catalogContent);
            InitializeCatalogViewModel(model);
            return model;
        }

        private LazyProductViewModelCollection CreateLazyProductContentViewModels(CatalogContentBase catalogContent)
        {
            return new LazyProductViewModelCollection(() =>
            {
                var products = GetChildrenAndRelatedEntries<ProductContent>(catalogContent);
                return products.Select(x => CreateProductViewModel(x));
            });
        }
        protected LazyVariationViewModelCollection CreateLazyVariantContentViewModels(CatalogContentBase catalogContent)
        {
            return new LazyVariationViewModelCollection(() =>
            {
                var variants = GetChildrenAndRelatedEntries<VariationContent>(catalogContent);
                return variants.Select(x => CreateVariationViewModel<VariationContent>(x));
            });
        }

        public IProductViewModel<ProductContent> CreateProductViewModel(ProductContent productContent)
        {
            var activator = new Activator<IProductViewModel<ProductContent>>();
            var model = activator.Activate(typeof(ProductViewModel<>), productContent);
            InitializeCatalogViewModel(model);
            return model;
        }

        public IVariationViewModel<TContent> CreateVariationViewModel<TContent>(TContent variationContent)
            where TContent : VariationContent            
        {
            var activator = new Activator<IVariationViewModel<TContent>>();
            var model = activator.Activate(typeof(VariationViewModel<TContent>), variationContent);
            InitializeVariationViewModel(model);
            return model;
        }

        public ContentArea CreateRelatedProductsContentArea(EntryContentBase catalogContent, string associationType)
        {
            IEnumerable<Association> associations = LinksRepository.GetAssociations(catalogContent.ContentLink);

            var relatedEntires = associations.Where(p => p.Group.Name.Equals(associationType))
                    .Where(x => x.Target != null && x.Target != ContentReference.EmptyReference)
                    .Select(x => ContentLoader.Get<EntryContentBase>(x.Target));

            var relatedEntriesCa = new ContentArea();

            foreach (var relatedEntire in relatedEntires)
            {
                ContentAreaItem caItem = new ContentAreaItem();
                caItem.ContentLink = relatedEntire.ContentLink;
                relatedEntriesCa.Items.Add(caItem);
            }

            return relatedEntriesCa;
        }

        protected List<ProductListViewModel> CreateProductListViewModels(IDictionary<string, IEnumerable<Recommendation>> result, string widgetName, int count)
        {
            if (result.ContainsKey(widgetName))
            {
                return ProductService.GetProductListViewModels(result[widgetName], count).ToList();
            }

            return new List<ProductListViewModel>();
        }

        private IEnumerable<TEntryContent> GetChildrenAndRelatedEntries<TEntryContent>(CatalogContentBase catalogContent)
            where TEntryContent : EntryContentBase
        {
            var variantContentItems = ContentLoader.GetChildren<TEntryContent>(catalogContent.ContentLink).ToList();

            var variantContainer = catalogContent as IVariantContainer;
            if (variantContainer != null)
            {
                variantContentItems.AddRange(GetRelatedEntries<TEntryContent>(variantContainer));
            }

            return variantContentItems.Where(e => e.IsAvailableInCurrentMarket(CurrentMarket));
        }

        private IEnumerable<TEntryContent> GetRelatedEntries<TEntryContent>(IVariantContainer content)
            where TEntryContent : EntryContentBase
        {
            var relatedItems = content.GetVariantRelations(LinksRepository).Select(x => x.Target);
            return ContentLoader.GetItems(relatedItems, LanguageSelector.AutoDetect()).OfType<TEntryContent>();
        }


        private Lazy<Inventory> GetInventory(IStockPlacement stockPlacement, string warehouseCode)
        {
            return new Lazy<Inventory>(() => stockPlacement.GetStockPlacement(InventoryLoader).FirstOrDefault(x => x.WarehouseCode == warehouseCode), true);
        }

        protected Price GetPrice(IPricing pricing)
        {
            return pricing.GetPrices(PricingLoader).FirstOrDefault(x => x.MarketId == CurrentMarket.GetCurrentMarket().MarketId);
        }

        protected bool IsSellable(VariationContent variationContent)
        {
            var inventory = InventoryService.GetForDefaultWarehouse(variationContent.Code);

            return HasPrice(variationContent) &&
                inventory != null &&
                inventory.PurchaseAvailableQuantity > 0;
        }

        protected bool HasPrice(VariationContent variationContent)
        {
            var price = variationContent
                .GetPrices(PricingLoader)
                .FirstOrDefault(x => x.MarketId == CurrentMarket.GetCurrentMarket().MarketId);
            return price != null &&
                price.UnitPrice != null;
        }

        public ActionResult Rss(EntryContentBase currentContent)
        {

            if (Request.Url != null)
            {
                string pageBaseUrl = string.Format("{0}://{1}{2}", "https", Request.Url.Host,
                    Request.Url.IsDefaultPort ? string.Empty : ":" + Request.Url.Port);
                
                var imageUrl = string.Empty;
                if (currentContent.CommerceMediaCollection.Any())
                {
                    imageUrl = Url.ContentUrl(currentContent.CommerceMediaCollection.First().AssetLink);
                }

                var description = string.Empty;
                if (currentContent["Overview"] != null)
                {
                    description = ((XhtmlString) currentContent["Overview"]).ToHtmlString().StripHtml();
                }

                var feed = new SyndicationFeed(currentContent.Name, description, new Uri(Request.Url.AbsoluteUri), null)
                {                    
                    ImageUrl = new Uri(pageBaseUrl + imageUrl), 
                };

                return new FeedResult(new Rss20FeedFormatter(feed));
            }
            return null;
        }
    }
}
