/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Globalization;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Business.Analytics;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.Blocks.Contracts;
using OxxCommerceStarterKit.Web.Models.Catalog;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class ProductSearchBlockController : BlockController<ProductSearchBlock>
    {

        private readonly IContentLoader _contentLoader;
        private readonly ProductService _productService;

        public class ProductSearchResult
        {
            public string Heading { get; set; }
            public List<ProductListViewModel> Products { get; set; }
        }

        public ProductSearchBlockController(IContentLoader contentLoader, ProductService productService)
        {
            _contentLoader = contentLoader;
            _productService = productService;

        }

        // GET: RelatedProductsBlock
        public override ActionResult Index(ProductSearchBlock currentContent)
        {
            // We need to know which language the page we're hosted on is
            string language = ControllerContext.RequestContext.GetLanguage();

            List<ProductListViewModel> productListViewModels;
            try
            {
                productListViewModels = currentContent.GetSearchResults(language);
            }
            catch (ServiceException)
            {
                return View("FindError");
            }

            if (productListViewModels == null)
            {
                // No hits, but we could still have manually added products
                productListViewModels = new List<ProductListViewModel>();
            }

            // Override result with priority products
            MergePriorityProducts(currentContent, productListViewModels);
            
            // Editor could let this one be empty
            int resultsPerPage = currentContent.ResultsPerPage;
            if(resultsPerPage == 0)
            {
                resultsPerPage = 1000; // Default Find limit
            }

            if (productListViewModels.Count > resultsPerPage)
            {
                productListViewModels = productListViewModels.Take(resultsPerPage).ToList();
            }

            if(productListViewModels.Any() == false)
            {
                return View("EmptyResult");
            }

            ProductSearchResult productSearchResult = new ProductSearchResult
            {
                Heading = currentContent.Heading,
                Products = productListViewModels
            };

            // Track impressions
            TrackProductImpressions(productSearchResult);

            return View(productSearchResult);
        }

        private void MergePriorityProducts(ProductSearchBlock currentContent, List<ProductListViewModel> productListViewModels)
        {
            if (currentContent.PriorityProducts != null)
            {
                foreach (var contentAreaItem in currentContent.PriorityProducts.Items)
                {
                    var item = contentAreaItem.GetContent();
                    if (item != null)
                    {
                        EntryContentBase entryContent = _contentLoader.Get<EntryContentBase>(item.ContentLink);
                        if (entryContent != null)
                        {
                            var viewModelInitializer = entryContent as IProductListViewModelInitializer;
                            if (viewModelInitializer != null)
                            {
                                // Remove priority products from list
                                productListViewModels.RemoveAll(
                                    x => x.ContentLink.CompareToIgnoreWorkID(entryContent.ContentLink));
                                // Add to beginning
                                ProductListViewModel priorityProduct =
                                    _productService.GetProductListViewModel(viewModelInitializer);
                                productListViewModels.Insert(0, priorityProduct);
                                
                            }
                        }
                    }
                }
            }
        }

        private void TrackProductImpressions(ProductSearchResult productSearchResult)
        {
            foreach (var product in productSearchResult.Products)
            {
                GoogleAnalyticsTracking tracker = new GoogleAnalyticsTracking(ControllerContext.HttpContext);
                tracker.ProductImpression(
                    product.Code,
                    product.DisplayName,
                    null,
                    product.BrandName,
                    null,
                    "Product Search Block");

            }
        }
    }
}
