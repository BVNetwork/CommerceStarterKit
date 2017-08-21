using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Web.Mvc;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using OxxCommerceStarterKit.Core;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Business.Analytics;
using OxxCommerceStarterKit.Web.Extensions;
using OxxCommerceStarterKit.Web.Models.Catalog;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
    [TemplateDescriptor(Inherited = true)]
    [RequireClientResources]
    public class GenericSizeVariationContentController : CommerceControllerBase<GenericSizeVariationContent>
    {
        private readonly ICurrentMarket _currentMarket;
        private readonly ProductService _productService;
        private readonly IRecommendationsService _recommendationsService;


        public GenericSizeVariationContentController(
            ICurrentMarket currentMarket, 
            ProductService productService,
            IRecommendationsService recommendationsService)
        {
            _currentMarket = currentMarket;
            _productService = productService;
            _recommendationsService = recommendationsService;
        }

        // GET: DigitalCameraSkuContent
        public ActionResult Index(GenericSizeVariationContent currentContent)
        {
            if (currentContent == null) throw new ArgumentNullException("currentContent");

            IVariationViewModel<GenericSizeVariationContent> viewModel = CreateVariationViewModel<GenericSizeVariationContent>(currentContent);

            viewModel.Media = GetMedia(currentContent);
            viewModel.PriceViewModel = currentContent.GetPriceModel();
            viewModel.AllVariationSameStyle = CreateRelatedVariationViewModelCollection(currentContent, Constants.AssociationTypes.SameStyle);

            var result = _recommendationsService.GetRecommendationsForProductPage(currentContent.Code, HttpContext);
            if (viewModel.RelatedProductsContentArea == null && result.ContainsKey("productCrossSellsWidget"))
            {
                viewModel.RelatedProductsContentArea = CreateRelatedProductsContentArea(result["productCrossSellsWidget"].Select(x => x.ContentLink));
            }

            if (result.ContainsKey("productAlternativesWidget"))
            {
                viewModel.ProductAlternatives = _productService.GetProductListViewModels(result["productAlternativesWidget"], 3).ToList();
            }
            else
            {
                viewModel.ProductAlternatives = new List<ProductListViewModel>();
            }

            viewModel.CartItem = new CartItemModel(currentContent) { CanBuyEntry = true };
            TrackAnalytics(viewModel);

            viewModel.IsSellable = IsSellable(currentContent);
            return View(viewModel);
        }
      
        private List<MediaData> GetMedia(GenericSizeVariationContent currentContent)
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var mediaReferences = currentContent.AssetImageUrls();
            List<MediaData> mediaData = new List<MediaData>();
            foreach (ContentReference mediaReference in mediaReferences)
            {
                MediaData file;
                if (contentLoader.TryGet<MediaData>(mediaReference, out file))
                {
                    mediaData.Add(file);
                }
            }
            return mediaData;
        }

        IEnumerable<IVariationViewModel<VariationContent>> CreateRelatedVariationViewModelCollection(CatalogContentBase catalogContent, string associationType)
        {
            IEnumerable<Association> associations = LinksRepository.GetAssociations(catalogContent.ContentLink);
            IEnumerable<IVariationViewModel<VariationContent>> productViewModels =
                Enumerable.Where(associations, p => p.Group.Name.Equals(associationType) && IsVariation<VariationContent>(p.Target))
                    .Select(a => CreateVariationViewModel(ContentLoader.Get<VariationContent>(a.Target)));

            return productViewModels;
        }


        private bool IsVariation<T>(ContentReference target) where T : VariationContent
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

        protected void TrackAnalytics(IVariationViewModel<GenericSizeVariationContent> viewModel)
        {
            // Track
            GoogleAnalyticsTracking tracking = new GoogleAnalyticsTracking(ControllerContext.HttpContext);
            tracking.ClearInteractions();

            // Track the main product view
            tracking.ProductAdd(
                viewModel.CatalogContent.Code,
                viewModel.CatalogContent.DisplayName,
                null,
                null,
                null, null, 0,
                (double)viewModel.CatalogContent.GetDefaultPriceAmountWholeNumber(_currentMarket.GetCurrentMarket()),
                0
                );

            // TODO: Track related products as impressions

            // Track action as details view
            tracking.Action("detail");
        }
    }
}