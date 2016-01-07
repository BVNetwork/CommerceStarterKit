using System;
using System.Collections.Generic;
using System.Globalization;
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

        public class RecommendedResult
        {
            public string Heading { get; set; }
            public List<ProductListViewModel> Products { get; set; }
        }

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
            RecommendedResult recommendedResult = new RecommendedResult();
            var currentCustomer = CustomerContext.Current.CurrentContact;
            int maxCount = 6;
            if (currentBlock.MaxCount > 0)
                maxCount = currentBlock.MaxCount;
            try
            {
                var recommendedProducts =
                    _recommendationService.GetRecommendedProducts(_currentCustomerService.GetCurrentUserId(), maxCount,
                        currentCulture);
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
                recommendedResult.Heading = currentBlock.Heading;
                recommendedResult.Products = models;
            }
            catch (Exception e)
            {
                //TODO:Log
            }
            
            
            return View("_recommendedProductsBlock", recommendedResult);
        }
    }
}