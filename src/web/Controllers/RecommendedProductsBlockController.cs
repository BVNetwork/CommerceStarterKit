using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Web.Mvc;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Business.Analytics;
using OxxCommerceStarterKit.Web.Business.Recommendations;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class RecommendedProductsBlockController : BlockController<RecommendedProductsBlock>
    {

        private readonly ProductService _productService;
        private readonly IRecommendationService _recommendationsService;

        public RecommendedProductsBlockController(ProductService productService, IRecommendationService recommendationsService)
        {
            _productService = productService;

            _recommendationsService = recommendationsService;
        }

        public override ActionResult Index(RecommendedProductsBlock currentBlock)
        {
           
            var result = _recommendationsService.GetRecommendationsForHomePage(HttpContext, (IContent)currentBlock)?.ToList() ?? new List<Recommendation>();

            if (result.Count < 3 && currentBlock.FallBackProducts != null)
            {
                result.AddRange(currentBlock.FallBackProducts.Select(x => new Recommendation(0, x)));
            }
            
            var productViewModels = _productService.GetProductListViewModels(result, 3).ToList();

            var recommendedResult = new RecommendedResult
            {
                Heading = currentBlock.Heading,
                Products = productViewModels
            };

            //TrackGoogleAnalyticsImpressions(currentBlock, productViewModels);

            return View("_recommendedProductsBlock", recommendedResult);
        }


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


        private void TrackGoogleAnalyticsImpressions(RecommendedProductsBlock currentBlock, List<ProductListViewModel> productViewModels)
        {
            foreach (var model in productViewModels)
            {
                //  model.TrackingName = recommendedProducts.RecommenderName;


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
    }
}