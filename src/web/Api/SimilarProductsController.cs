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
        private readonly IRecommendationService _recommendationService;
        private readonly IContentLoader _contentLoader;

        public SimilarProductsController(IRecommendationService recommendationService, IContentLoader contentLoader)
        {
            _recommendationService = recommendationService;
            _contentLoader = contentLoader;
        }


        [HttpGet]
        public object GetSimilarProducts(string indexId)
        {
            try
            {
                SetLanguage();
                string language = Language;
                var client = SearchClient.Instance;
                //Sannsyn test
                FindProduct currentProduct = client.Search<FindProduct>()
                    .Filter(x => x.IndexId.Match(indexId)).Take(1).StaticallyCacheFor(TimeSpan.FromMinutes(1)).GetResult().FirstOrDefault();
                List<SimilarProductObject> similarProductObjects = new List<SimilarProductObject>();
                if (currentProduct != null)
                {

                    List<string> productCodes = _recommendationService.GetRecommendationsForProduct(currentProduct.Code).ToList();
                    if (productCodes.Any())
                    {

                        var result = client.Search<FindProduct>()
                        .Filter(x => x.Code.In(productCodes))
                        .Filter(x => x.Language.Match(language))
                        .StaticallyCacheFor(TimeSpan.FromMinutes(1))
                        .GetResult();
                        similarProductObjects = result.Select(y => new SimilarProductObject
                        {
                            Name = y.DisplayName,
                            Image = y.DefaultImageUrl,
                            Url = y.ProductUrl,
                            DefaultPrice = y.DefaultPrice,
                            DiscountedPrice = y.DiscountedPrice
                        }).ToList();
                        return similarProductObjects;
                    }
                }


                //var client = SearchClient.Instance;
                //FindProduct currentProduct = client.Search<FindProduct>()
                //    .Filter(x => x.IndexId.Match(indexId)).Take(1).StaticallyCacheFor(TimeSpan.FromMinutes(1)).GetResult().FirstOrDefault();
                //if (currentProduct != null)
                //{

                //    List<SimilarProductObject> similarProductObjects = new List<SimilarProductObject>();

                //    var result = client.Search<FindProduct>()
                //        .Filter(x => x.CategoryName.Match(currentProduct.CategoryName))
                //        .Filter(x => x.MainCategoryName.Match(currentProduct.MainCategoryName))
                //        .Filter(x => !x.DisplayName.Match(currentProduct.DisplayName))
                //        .Filter(x => x.Language.Match(language))
                //        .StaticallyCacheFor(TimeSpan.FromMinutes(1))
                //        .GetResult();

                //    similarProductObjects = result.Select(y => new SimilarProductObject
                //    {
                //        Name = y.DisplayName,
                //        Image = y.DefaultImageUrl,
                //        Url = y.ProductUrl,
                //        DefaultPrice = y.DefaultPrice,
                //        DiscountedPrice = y.DiscountedPrice
                //    }).ToList();


                //    return similarProductObjects;
                //}
                return null;
            }
            catch (ServiceException)
            {
                return null;
            }

        }
    }
}
