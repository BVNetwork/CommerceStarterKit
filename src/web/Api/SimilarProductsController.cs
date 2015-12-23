/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Framework;
using Mediachase.Commerce.Catalog;
using OxxCommerceStarterKit.Web.Models.FindModels;
using Sannsyn.Episerver.Commerce.Services;
using EPiServer.Find.Commerce;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Interfaces;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Api
{

    public class SimilarProductObject
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
        public string DefaultPrice { get; set; }
        public string DiscountedPrice { get; set; }
    }

    public class SimilarProductsController : BaseApiController
    {

        private readonly IRecommendedProductsService _recommendationService;
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;
        private readonly ProductService _productService;
        private readonly ICurrentCustomerService _currentCustomerService;

        private readonly ICurrentMarket _currentMarket;

        public SimilarProductsController(IRecommendedProductsService recommendationService,
            IContentLoader contentLoader,
            ICurrentCustomerService currentCustomerService,
            ICurrentMarket currentMarket,
            ReferenceConverter referenceConverter,
            ProductService productService)
        {
            _recommendationService = recommendationService;
            _contentLoader = contentLoader;
            _currentCustomerService = currentCustomerService;
            _currentMarket = currentMarket;
            _referenceConverter = referenceConverter;
            _productService = productService;
        }


        [HttpGet]
        public IEnumerable<ProductListViewModel> GetSimilarProducts(int contentId)
        {
            const int maxRecommendedProducts = 10;
            List<ProductListViewModel> models = new List<ProductListViewModel>();
            SetLanguage();
            string language = Language;
            ContentReference contentLink = _referenceConverter.GetContentLink(contentId, CatalogContentType.CatalogEntry, 0);
            IContent contentItem;

            if (_contentLoader.TryGet(contentLink, out contentItem))
            {
                EntryContentBase entryContent = contentItem as EntryContentBase;
                if (entryContent != null)
                {
                    var currentCustomer = CustomerContext.Current.CurrentContact;
                    var recommendedProducts = _recommendationService.GetRecommendedProducts(entryContent,
                        _currentCustomerService.GetCurrentUserId(), maxRecommendedProducts);


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
                                if(model == null)
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
                }
            }

            return models;
        }
    }
}
