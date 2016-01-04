using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Globalization;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Interfaces;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using Mediachase.Commerce;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class RecommendedProductsBlockController : BlockController<RecommendedProductsBlock>
    {
        private readonly IRecommendedProductsService _recommendationService;
        private readonly ICurrentCustomerService _currentCustomerService;
        private readonly ICurrentMarket _currentMarket;
        private readonly ProductService _productService;

        public RecommendedProductsBlockController(IRecommendedProductsService recommendationService, 
            ICurrentCustomerService currentCustomerService, 
            ICurrentMarket currentMarket,
            ProductService productService)
        {
            _recommendationService = recommendationService;
            _currentCustomerService = currentCustomerService;
            _currentMarket = currentMarket;
            _productService = productService;
        }

        public override ActionResult Index(RecommendedProductsBlock currentBlock)
        {
            CultureInfo currentCulture = ContentLanguage.PreferredCulture;
            List<ProductListViewModel> models = new List<ProductListViewModel>();
            var recommendedProducts  = _recommendationService.GetRecommendedProducts(_currentCustomerService.GetCurrentUserId(), 10, currentCulture);
            var currentCustomer = CustomerContext.Current.CurrentContact;
            foreach (var content in recommendedProducts)
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

                        // Fallback
                        if (model == null)
                        {
                            model = new ProductListViewModel(product, _currentMarket.GetCurrentMarket(),
                                currentCustomer);
                        }
                    }
                }

                if (model != null)
                {
                    models.Add(model);
                }

            }
            return View("_recommendedProductsBlock", models);
        }
    }
}