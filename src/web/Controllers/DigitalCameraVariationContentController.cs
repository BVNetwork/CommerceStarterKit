using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Web.Mvc;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
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

    public interface IProductRecommendationService
    {
        IDictionary<string, IEnumerable<ContentReference>> GetProductRecommendations(string productCode,
            HttpContextBase context);
    }

    [ServiceConfiguration(typeof(IProductRecommendationService))]
    public class ProductRecommentationService : IProductRecommendationService
    {
        private readonly ITrackingService _trackingService;
        private readonly TrackingDataFactory _trackingDataFactory;
        private readonly ReferenceConverter _referenceConverter;

        public ProductRecommentationService(ITrackingService trackingService,
            TrackingDataFactory trackingDataFactory,
            ReferenceConverter referenceConverter)
        {
            _trackingService = trackingService;
            _trackingDataFactory = trackingDataFactory;
            _referenceConverter = referenceConverter;
        }

        public IDictionary<string, IEnumerable<ContentReference>> GetProductRecommendations(string productCode, HttpContextBase context)
        {
            var returnValue = new Dictionary<string, IEnumerable<ContentReference>>();

            var trackingData = _trackingDataFactory.CreateProductTrackingData(productCode, context);
            var result = _trackingService.Send(trackingData, context);

            if (result.SmartRecs != null)
            {
                foreach (var recommendation in result.SmartRecs)
                {
                    returnValue.Add(recommendation.Widget, recommendation.Recs.Select(x => _referenceConverter.GetContentLink(x.RefCode)));
                }
            }

            return returnValue;
        }
    }

    [TemplateDescriptor(Inherited = true)]
    [RequireClientResources]
    public class DigitalCameraVariationContentController : CommerceControllerBase<DigitalCameraVariationContent>
    {
         private readonly ICurrentMarket _currentMarket;
        private LocalizationService _localizationService;
        private ReadOnlyPricingLoader _readOnlyPricingLoader;
        private readonly IPriceDetailService _priceDetailService;
        private readonly ProductService _productService;
        private readonly IProductRecommendationService _productRecommendationService;

        public DigitalCameraVariationContentController()
			: this(ServiceLocator.Current.GetInstance<LocalizationService>(),
			ServiceLocator.Current.GetInstance<ReadOnlyPricingLoader>(),
			ServiceLocator.Current.GetInstance<ICurrentMarket>(),
            ServiceLocator.Current.GetInstance<IPriceDetailService>(),
            ServiceLocator.Current.GetInstance<ProductService>(),
            ServiceLocator.Current.GetInstance<IProductRecommendationService>()
			)
		{
		}
        public DigitalCameraVariationContentController(LocalizationService localizationService, 
            ReadOnlyPricingLoader readOnlyPricingLoader, 
            ICurrentMarket currentMarket, 
            IPriceDetailService priceDetailService, 
            ProductService productService,
            IProductRecommendationService productRecommendationService)
        {
            _localizationService = localizationService;
            _readOnlyPricingLoader = readOnlyPricingLoader;
            _currentMarket = currentMarket;
            _priceDetailService = priceDetailService;
            _productService = productService;
            _productRecommendationService = productRecommendationService;
        }

        
        // GET: DigitalCameraSkuContent
        public ActionResult Index(DigitalCameraVariationContent currentContent)
        {
            if (currentContent == null) throw new ArgumentNullException("currentContent");

            var recs = _productRecommendationService.GetProductRecommendations(currentContent.Code, HttpContext);

            DigitalCameraVariationViewModel viewModel = new DigitalCameraVariationViewModel(currentContent);
          
            viewModel.PriceViewModel = currentContent.GetPriceModel();
            viewModel.AllVariationSameStyle = CreateRelatedVariationViewModelCollection(currentContent, Constants.AssociationTypes.SameStyle);

            if (viewModel.RelatedProductsContentArea == null && recs.ContainsKey("productCrossSellsWidget"))
            {
                viewModel.RelatedProductsContentArea = CreateRelatedProductsContentArea(recs["productCrossSellsWidget"]);
            }

            if (recs.ContainsKey("productAlternativesWidget"))
            {
                viewModel.ProductAlternatives =
                    _productService.GetProductListViewModels(recs["productAlternativesWidget"], 3).ToList();
            }
            else
            {
                viewModel.ProductAlternatives = new List<ProductListViewModel>();
            }

            TrackAnalytics(viewModel);

            viewModel.IsSellable = IsSellable(currentContent);
            return View(viewModel);
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

        protected void TrackAnalytics(DigitalCameraVariationViewModel viewModel)
        {
            // Track
            GoogleAnalyticsTracking tracking = new GoogleAnalyticsTracking(ControllerContext.HttpContext);
            tracking.ClearInteractions();

            // Track the main product view
            tracking.ProductAdd(
                viewModel.CatalogVariationContent.Code,
                viewModel.CatalogVariationContent.DisplayName,
                null,
                viewModel.CatalogVariationContent.Facet_Brand,
                null, null, 0,
                (double)viewModel.CatalogVariationContent.GetDefaultPriceAmountWholeNumber(_currentMarket.GetCurrentMarket()),
                0
                );

            // TODO: Track related products as impressions

            // Track action as details view
            tracking.Action("detail");
        }
    }
}