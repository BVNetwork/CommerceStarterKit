/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Recommendations.Commerce.Tracking;
using Mediachase.Commerce;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Services
{
    public class ProductService
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICurrentMarket _currentMarket;

        public ProductService(IContentLoader contentLoader, ICurrentMarket currentMarket)
        {
            _contentLoader = contentLoader;
            _currentMarket = currentMarket;
        }


        public ProductListViewModel GetProductListViewModel(IProductListViewModelInitializer productContent)
        {
            if (productContent != null)
                return productContent.Populate(_currentMarket.GetCurrentMarket());
            return null;
        }


        public IEnumerable<ProductListViewModel> GetProductListViewModels(IEnumerable<Recommendation> contentReferences, int maxCount = 6)
        {

            foreach (var reference in contentReferences.Take(maxCount))
            {
                CatalogContentBase content;

                if (!_contentLoader.TryGet(reference.ContentLink, out content))
                    continue;

                var productListViewModelInitializer = content as IProductListViewModelInitializer;

                if (productListViewModelInitializer == null)
                    continue;

                var model = GetProductListViewModel(productListViewModelInitializer);
                model.RecommendationId = reference.RecommendationId.ToString();

                yield return model;
            }
        }
    }
}
