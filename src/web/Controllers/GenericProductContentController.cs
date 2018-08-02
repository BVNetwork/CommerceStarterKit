using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Web.Mvc;
using OxxCommerceStarterKit.Core;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Business.Recommendations;
using OxxCommerceStarterKit.Web.Extensions;
using OxxCommerceStarterKit.Web.Models.Catalog;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using SelectListItem = OxxCommerceStarterKit.Web.Models.ViewModels.SelectListItem;

namespace OxxCommerceStarterKit.Web.Controllers
{
    [TemplateDescriptor(Inherited = true)]
    [RequireClientResources]
    public class GenericProductContentController : CommerceControllerBase<GenericProductContent>
    {
		private readonly LocalizationService _localizationService;
        private readonly IRecommendationService _recommendationService;

        public GenericProductContentController(LocalizationService localizationService, IRecommendationService recommendationService)
		{			
			_localizationService = localizationService;
		    _recommendationService = recommendationService;
		}

        public ViewResult Index(GenericProductContent currentContent, HomePage currentPage, string size)
        {
            var model = GetProductViewModel(currentContent, currentPage, size);

            var result = _recommendationService.GetRecommendationsForProductPage(currentContent.Code, HttpContext, currentContent);
            model.ProductCrossSell = CreateProductListViewModels(result, "productCrossSellsWidget", 6);
            model.ProductAlternatives = CreateProductListViewModels(result, "productAlternativesWidget", 4);

            return View(model);
        }

        private GenericProductViewModel GetProductViewModel(GenericProductContent currentContent, HomePage currentPage, string size)
        {
            var model = CreateGenericProductViewModel(currentContent, currentPage);

            var variationItems = GetProductVariants(model);
            // Get current fashion item
            if (variationItems.Any())
            {
                var variationItem = GetSelectedItem(size, variationItems);
                if (variationItem != null)
                {
                    model.GenericVariationViewModel = CreateVariationViewModel(variationItem);
                    model.GenericVariationViewModel.PriceViewModel = variationItem.GetPriceModel();
                    model.ContentWithAssets = currentContent;
                }
            }

            CreateSizeListItems(model, variationItems);

            // check if this product is sellable
            model.IsSellable = model.GenericVariationViewModel != null && IsSellable(model.GenericVariationViewModel.CatalogContent);

            return model;
        }

        public GenericProductViewModel CreateGenericProductViewModel(GenericProductContent currentContent, HomePage currentPage)
        {
            var model = new GenericProductViewModel(currentContent);
            if (!string.IsNullOrWhiteSpace(currentContent.BuyButtonColor))
            {
                model.BuyButtonColor = currentContent.BuyButtonColor;
            }
            InitializeProductViewModel(model);

            //// get delivery and returns from the start page
            //var startPage = ContentLoader.Get<HomePage>(ContentReference.StartPage);
            //model.DeliveryAndReturns = startPage.Settings.DeliveryAndReturns;

            //model.SizeGuideReference = GetSizeGuide(currentContent);

            //model.SizeUnit = currentContent.SizeUnit;
            //model.SizeType = currentContent.SizeType;

            return model;
        }

        public void InitializeProductViewModel<TViewModel>(TViewModel model)
           where TViewModel : ICatalogViewModel<ProductContent>
        {
            model.ChildCategories = model.ChildCategories ?? GetCatalogChildNodes(model.CatalogContent.ContentLink);

            model.AllProductsSameStyle = CreateRelatedProductContentViewModels(model.CatalogContent, Constants.AssociationTypes.SameStyle);
         
            model.Variants = model.Variants ?? CreateLazyVariantContentViewModels(model.CatalogContent);

            if (model.RelatedProductsContentArea == null)
            {
                model.RelatedProductsContentArea = CreateRelatedProductsContentArea(model.CatalogContent, Constants.AssociationTypes.Default);
            }

        }

       

        private IEnumerable<IProductViewModel<ProductContent>> CreateRelatedProductContentViewModels(CatalogContentBase catalogContent, string associationType)
        {
            IEnumerable<Association> associations = AssociationRepository.GetAssociations(catalogContent.ContentLink);

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
        
        private List<GenericSizeVariationContent> GetProductVariants(GenericProductViewModel model)
        {
            var variationItems = model.Variants.Value
                .Select(x => x.CatalogContent)
                .OfType<GenericSizeVariationContent>()
                .ToList();
            return variationItems;
        }

        private GenericSizeVariationContent GetSelectedItem(string size, IEnumerable<GenericSizeVariationContent> variationItems)
        {
            bool sizeChosen = !string.IsNullOrEmpty(size);
            if (sizeChosen)
            {
                variationItems = variationItems.Where(x => x.Size == size);
            }

            if (!sizeChosen)
            {
                // get first that is sellable -- This might be slow
                var hasPriceAndInventory = variationItems.FirstOrDefault(x => IsSellable(x));
                if (hasPriceAndInventory != null)
                {
                    return hasPriceAndInventory;
                }
            }
            return variationItems.FirstOrDefault();
        }

        private void CreateSizeListItems(GenericProductViewModel model, List<GenericSizeVariationContent> variationItems)
        {
            if (model.GenericVariationViewModel != null)
            {
                List<SelectListItem> items = variationItems.Select(x =>
                        {
                            var inventory = InventoryService.GetForDefaultWarehouse(x.Code);
                            bool inStock = inventory != null && inventory.PurchaseAvailableQuantity > 0;
                            return CreateSelectListItem(x.Size, x.Size + GetStockText(inStock), !inStock, x.Code);
                        }).ToList();

                model.Size = items;
            }
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
    }
}