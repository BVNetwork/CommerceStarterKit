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
using OxxCommerceStarterKit.Web.Business.Recommendations;
using OxxCommerceStarterKit.Web.Extensions;
using OxxCommerceStarterKit.Web.Models.Catalog;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    [TemplateDescriptor(Inherited = true)]
    [RequireClientResources]
    public class GenericSizeVariationContentController : CommerceControllerBase<GenericSizeVariationContent>
    {
        private readonly ICurrentMarket _currentMarket;
        private readonly IRecommendationService _recommendationService;


        public GenericSizeVariationContentController(ICurrentMarket currentMarket, IRecommendationService recommendationService)
        {
            _currentMarket = currentMarket;
            _recommendationService = recommendationService;
        }

        // GET: DigitalCameraSkuContent
        public ActionResult Index(GenericSizeVariationContent currentContent)
        {
            if (currentContent == null) throw new ArgumentNullException("currentContent");

            var viewModel = CreateVariationViewModel(currentContent);

            viewModel.Media = GetMedia(currentContent);
            viewModel.PriceViewModel = currentContent.GetPriceModel();
            viewModel.AllVariationSameStyle = CreateRelatedVariationViewModelCollection(currentContent, Constants.AssociationTypes.SameStyle);

            var result = _recommendationService.GetRecommendationsForProductPage(currentContent.Code, HttpContext, currentContent);
            viewModel.ProductCrossSell = CreateProductListViewModels(result, "productCrossSellsWidget", 6);
            viewModel.ProductAlternatives = CreateProductListViewModels(result, "productAlternativesWidget", 3);

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
                if (contentLoader.TryGet(mediaReference, out file))
                {
                    mediaData.Add(file);
                }
            }
            return mediaData;
        }

        IEnumerable<IVariationViewModel<VariationContent>> CreateRelatedVariationViewModelCollection(CatalogContentBase catalogContent, string associationType)
        {
            IEnumerable<Association> associations = LinksRepository.GetAssociations(catalogContent.ContentLink);

            var productViewModels = associations
                .Where(p => p.Group.Name.Equals(associationType) && IsVariation<VariationContent>(p.Target))
                .Select(a => CreateVariationViewModel(ContentLoader.Get<VariationContent>(a.Target)));

            return productViewModels;
        }


        private bool IsVariation<T>(ContentReference target) where T : VariationContent
        {
            T content;
            if (ContentLoader.TryGet(target, out content))
            {
                var contents = new List<T> { content };
                var c = contents.FilterForDisplay().FirstOrDefault();
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
                null, 
                null, 
                0,
                viewModel.CatalogContent.GetDefaultPriceAmountWholeNumber(_currentMarket.GetCurrentMarket()));

            // TODO: Track related products as impressions

            // Track action as details view
            tracking.Action("detail");
        }
    }
}