/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Web.Mvc;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using OxxCommerceStarterKit.Core;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Web.Api;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Models.Catalog;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using SelectListItem = OxxCommerceStarterKit.Web.Models.ViewModels.SelectListItem;

namespace OxxCommerceStarterKit.Web.Controllers
{
    [TemplateDescriptor(Inherited = true)]
	[RequireClientResources]
	public class FashionProductContentController : CommerceControllerBase<FashionProductContent>
	{
		#region Variables and constructors
		
		private readonly LocalizationService _localizationService;
		private readonly ReadOnlyPricingLoader _readOnlyPricingLoader;
		private readonly ICurrentMarket _currentMarket;
	    private readonly IDefaultInventoryService _defaultInventoryService;

	    public FashionProductContentController()
			: this(ServiceLocator.Current.GetInstance<LocalizationService>(),
			ServiceLocator.Current.GetInstance<ReadOnlyPricingLoader>(),
			ServiceLocator.Current.GetInstance<ICurrentMarket>(),
            ServiceLocator.Current.GetInstance<IDefaultInventoryService>()
			)
		{
		}

		public FashionProductContentController(LocalizationService localizationService, 
            ReadOnlyPricingLoader readOnlyPricingLoader, 
            ICurrentMarket currentMarket,
            IDefaultInventoryService defaultInventoryService)
		{			
			
			_localizationService = localizationService;
			_readOnlyPricingLoader = readOnlyPricingLoader;
			_currentMarket = currentMarket;
		    _defaultInventoryService = defaultInventoryService;
		}
		#endregion

		public ViewResult Index(FashionProductContent currentContent, HomePage currentPage, string size, string color)
		{
			var model = GetProductViewModel(currentContent, currentPage, size, color);

			return View(model);
		}

		public ViewResult Product(string productId, HomePage currentPage)
		{
			var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
			ContentReference productLink = null;
			FashionProductViewModel model;

            // First we try the content link directly
            ContentReference.TryParse(productId, out productLink);
            if (ContentReference.IsNullOrEmpty(productLink))
            {
                // Then we try the code
                var referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
                try
                {
                    productLink = referenceConverter.GetContentLink(productId, CatalogContentType.CatalogEntry);
                }
                catch (Exception ex)
                {
                    // We cannot parse it
                    _log.Warning("Cannot parse " + productId + " as a product link or code", ex);
                    productLink = null;
                }
            }

			if (productLink != null)
			{
				var currentContent = contentLoader.Get<FashionProductContent>(productLink);
				string size = "";
				string color = "";
				currentPage = currentPage ?? contentLoader.Get<HomePage>(ContentReference.StartPage);

				model = GetProductViewModel(currentContent, currentPage, size, color);
			}
			else
			{
				model = new FashionProductViewModel(null);
			}

			return View("_Product", model);
		}

		private FashionProductViewModel GetProductViewModel(FashionProductContent currentContent, HomePage currentPage, string size, string color)
		{
			var model = CreateFashionProductViewModel(currentContent, currentPage);
			 

			var fashionItems = GetProductVariants(model);
			// Get current fashion item
			if (fashionItems.Any())
			{
				var fashionItem = GetFashionItem(size, color, fashionItems);
				if (fashionItem != null)
				{
					model.FashionItemViewModel =
						CreateVariationViewModel<FashionItemContent>(fashionItem);
                    model.FashionItemViewModel.PriceViewModel = GetPriceModel(fashionItem);
					model.ContentWithAssets = currentContent;
				}
			}
			CreateSizeListItems(model, fashionItems);

			// check if this product is sellable
			model.IsSellable = model.FashionItemViewModel != null && IsSellable(model.FashionItemViewModel.CatalogContent);
			
			return model;
		}

        public FashionProductViewModel CreateFashionProductViewModel(FashionProductContent currentContent, HomePage currentPage)
        {
            var model = new FashionProductViewModel(currentContent);
            InitializeProductViewModel(model);

            // get delivery and returns from the start page
            var startPage = ContentLoader.Get<HomePage>(ContentReference.StartPage);
            model.DeliveryAndReturns = startPage.Settings.DeliveryAndReturns;

            model.SizeGuideReference = GetSizeGuide(currentContent);

            model.SizeUnit = currentContent.SizeUnit;
            model.SizeType = currentContent.SizeType;

            return model;
        }

        public void InitializeProductViewModel<TViewModel>(TViewModel model)
            where TViewModel : ICatalogViewModel<ProductContent>
        {
            model.ChildCategories = model.ChildCategories ?? GetCatalogChildNodes(model.CatalogContent.ContentLink);
            //EntryContentBase parent = GetParent(model.CatalogContent);
            model.AllProductsSameStyle = CreateRelatedProductContentViewModels(model.CatalogContent, Constants.AssociationTypes.SameStyle);
            //model.Products = model.Products ?? CreateLazyProductContentViewModels(model.CatalogContent, model.CurrentPage);
            model.Variants = model.Variants ?? CreateLazyVariantContentViewModels(model.CatalogContent);

            if (model.RelatedProducts == null)
            {
                model.RelatedProducts = CreateRelatedProductContentViewModels(model.CatalogContent, Constants.AssociationTypes.Default);
            }

        }

        private IEnumerable<IProductViewModel<ProductContent>> CreateRelatedProductContentViewModels(CatalogContentBase catalogContent, string associationType)
        {
            IEnumerable<Association> associations = LinksRepository.GetAssociations(catalogContent.ContentLink);
            IEnumerable<IProductViewModel<ProductContent>> productViewModels =
                Enumerable.Where(associations, p => p.Group.Name.Equals(associationType) && IsProduct<ProductContent>(p.Target))
                    .Select(a => CreateProductViewModel(ContentLoader.Get<ProductContent>(a.Target)));

            return productViewModels;
        }


        private bool IsProduct<T>(ContentReference target) where T : ProductContent
        {
            T content;
            if (ContentLoader.TryGet<T>(target, out content))
            {
                List<T> contents = new List<T>();
                contents.Add(content);
                var c = contents.FilterForDisplay<T>().FirstOrDefault();
                return c != null;
            }
            return false;
        }

		private List<FashionItemContent> GetProductVariants(FashionProductViewModel model)
		{
			var fashionItems = model.Variants.Value
				.Select(x => x.CatalogContent)
				.OfType<FashionItemContent>()				
				.ToList();
			return fashionItems;
		}


		

		private void CreateSizeListItems(FashionProductViewModel model, List<FashionItemContent> fashionItems)
		{

			if (model.FashionItemViewModel != null)
			{
                List<SelectListItem> items =
					fashionItems.Distinct(new FashionProductSizeVariationComparer())
						.OrderBy(x => {
							return ShoppingController.SortSizes(model.SizeType + "/" + model.SizeUnit + "/" + x.Facet_Size);
						})
						.Select(x => {
							var inventory = _defaultInventoryService.GetForDefaultWarehouse(x.Code);
							bool inStock = inventory != null && inventory.PurchaseAvailableQuantity > 0;
							return CreateSelectListItem(x.Facet_Size, x.Facet_Size + GetStockText(inStock), !inStock, x.Code);
						}).ToList();

				model.Size = items;
			}
		}

		private class FashionProductSizeVariationComparer : IEqualityComparer<FashionItemContent>
		{
			public bool Equals(FashionItemContent x, FashionItemContent y)
			{
                return x != null && x.Facet_Size != null && y != null ? x.Facet_Size.Equals(y.Facet_Size) : false;
			}
			public int GetHashCode(FashionItemContent x)
			{
                return x != null && x.Facet_Size != null ? x.Facet_Size.GetHashCode() : -1;
			}
		}

		private FashionItemContent GetFashionItem(string size, string color, IEnumerable<FashionItemContent> fashionItems)
		{
			var fashionItemQuery = fashionItems;
			bool sizeChosen = !string.IsNullOrEmpty(size);
			if (sizeChosen)
			{
                fashionItemQuery = fashionItemQuery.Where(x => x.Facet_Size == size);
			}

			if (!string.IsNullOrEmpty(color))
			{
				fashionItemQuery = fashionItemQuery.Where(x => x.FacetColor == color);
			}

			if (!sizeChosen)
			{
				// get first that is sellable -- This might be slow
				var hasPriceAndInventory = fashionItemQuery.FirstOrDefault(x => IsSellable(x));
				if (hasPriceAndInventory != null)
				{
					return hasPriceAndInventory;
				}
			}
			return fashionItemQuery.FirstOrDefault();
		}

		private static SelectListItem CreateSelectListItem(string value, string text, bool disabled, string dataCode)
		{
			return new SelectListItem() { Text = text, Value = value, Disabled = disabled, DataCode = dataCode };
		}

		private string GetStockText(bool inStock)
		{
			return inStock ? "" /*InStockText(inventory)*/ :
				" - " + _localizationService.GetString("/common/product/out_of_stock");
		}

		//private string InStockText(IWarehouseInventory inventory)
		//{
		//	return " - " + (string)inventory.InStockQuantity.ToString() + " " + _localizationService.GetString("/common/product/in_stock");
		//}
	}
}
