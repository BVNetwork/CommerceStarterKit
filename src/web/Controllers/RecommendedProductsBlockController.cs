using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Interfaces;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using OxxCommerceStarterKit.Web.Business.Analytics;
using OxxCommerceStarterKit.Web.Services;
using Sannsyn.Episerver.Commerce;
using Sannsyn.Episerver.Commerce.Tracking;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class RecommendedProductsBlockController : BlockController<RecommendedProductsBlock>
    {
        private readonly IRecommendedProductsService _recommendationService;
        private readonly ICurrentCustomerService _currentCustomerService;
        private readonly ICurrentMarket _currentMarket;
        private readonly ProductService _productService;
        private readonly IContentLoader _contentLoader;
        private readonly ITrackingService _trackingService;
        private readonly TrackingDataFactory _trackingDataFactory;
        private readonly ReferenceConverter _referenceConverter;

        public class RecommendedResult
        {
            public string Heading { get; set; }
            public string TrackingName { get; set; }
            public List<ProductListViewModel> Products { get; set; }

            public string GetTrackingName(ProductListViewModel product)
            {
                if (string.IsNullOrEmpty(TrackingName) == false)
                {
                    return TrackingName + "_" + product.Code;
                }
                return string.Empty;
            }
        }

        public RecommendedProductsBlockController(IRecommendedProductsService recommendationService,
            ICurrentCustomerService currentCustomerService,
            ICurrentMarket currentMarket,
            ProductService productService,
            IContentLoader contentLoader,
            ITrackingService trackingService,
            TrackingDataFactory trackingDataFactory,
            ReferenceConverter referenceConverter)
        {
            _recommendationService = recommendationService;
            _currentCustomerService = currentCustomerService;
            _currentMarket = currentMarket;
            _productService = productService;
            _contentLoader = contentLoader;
            _trackingService = trackingService;
            _trackingDataFactory = trackingDataFactory;
            _referenceConverter = referenceConverter;
        }

        public override ActionResult Index(RecommendedProductsBlock currentBlock)
        {

            List<ProductListViewModel> models = new List<ProductListViewModel>();

            RecommendedResult recommendedResult = new RecommendedResult();
            var currentCustomer = CustomerContext.Current.CurrentContact;

            //   var recommendedProducts = GetRecommendedProducts(currentBlock);

            var trackingData = _trackingDataFactory.CreateHomeTrackingData(HttpContext);
            var result = _trackingService.Send(trackingData, HttpContext);


            var productRefs = result.SmartRecs
                .SelectMany(x => x.Recs)
                .Take(3)
                .Select(x => _referenceConverter.GetContentLink(x.RefCode));


            foreach (var content in productRefs.Select(x => _contentLoader.Get<CatalogContentBase>(x)))
            {
                ProductListViewModel model = null;
                VariationContent variation = content as VariationContent;
                if (variation != null)
                {
                    model = new ProductListViewModel(variation, _currentMarket.GetCurrentMarket(),
                        currentCustomer);
                }
                else
                {
                    ProductContent product = content as ProductContent;
                    if (product != null)
                    {
                        model = _productService.GetProductListViewModel(product as IProductListViewModelInitializer);
                        if (model == null)
                        {
                            model = new ProductListViewModel(product, _currentMarket.GetCurrentMarket(),
                                currentCustomer);
                        }
                    }
                }

                if (model != null)
                {
                  //  model.TrackingName = recommendedProducts.RecommenderName;

                    models.Add(model);

                    // Track
                 //   ControllerContext.HttpContext.AddRecommendationExposure(new TrackedRecommendation() { ProductCode = model.Code, RecommenderName = recommendedProducts.RecommenderName });
                    GoogleAnalyticsTracking tracker = new GoogleAnalyticsTracking(ControllerContext.HttpContext);
                    tracker.ProductImpression(
                        model.Code,
                        model.DisplayName,
                        null,
                        model.BrandName,
                        null,
                        currentBlock.Heading);

                }
            }

          //  recommendedResult.TrackingName = recommendedProducts.RecommenderName;
            recommendedResult.Heading = currentBlock.Heading;
            recommendedResult.Products = models;

            return View("_recommendedProductsBlock", recommendedResult);
        }

        private IRecommendations GetRecommendedProducts(RecommendedProductsBlock currentBlock)
        {
            IRecommendations recommendedProducts;
            CultureInfo currentCulture = ContentLanguage.PreferredCulture;
            int maxCount = 6;
            if (currentBlock.MaxCount > 0)
                maxCount = currentBlock.MaxCount;
            if (currentBlock.Category != null)
            {
                NodeContent catalogNode = _contentLoader.Get<NodeContent>(currentBlock.Category);
                var code = catalogNode.Code;
                recommendedProducts =
                    _recommendationService.GetRecommendedProductsByCategory(_currentCustomerService.GetCurrentUserId(),
                        new List<string> { code },
                        maxCount,
                        currentCulture);
            }
            else
            {
                recommendedProducts =
                _recommendationService.GetRecommendedProducts(_currentCustomerService.GetCurrentUserId(), maxCount,
                    currentCulture);
            }

            // If disabled, we won't get anything which could break the view
            if(recommendedProducts == null)
            {
                recommendedProducts = new Recommendations(string.Empty, new List<IContent>(0));
            }

            return recommendedProducts;
        }
    }
}