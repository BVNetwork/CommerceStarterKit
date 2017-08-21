using System;
using System.Collections.Generic;
using System.Globalization;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Interfaces;
using OxxCommerceStarterKit.Web.Models.FindModels;

namespace OxxCommerceStarterKit.Web.Services
{
    [ServiceConfiguration(typeof(IRecommendedProductsService))]
    public class RecommendedProductsService : IRecommendedProductsService
    {
        private readonly IContentRepository _contentRepository;
        private readonly ReferenceConverter _referenceConverter;

        public RecommendedProductsService(IContentRepository contentRepository, ReferenceConverter referenceConverter)
        {
            _contentRepository = contentRepository;
            _referenceConverter = referenceConverter;
        }

        public IRecommendations GetRecommendedProducts(EntryContentBase catalogEntry, string userId, int maxCount)
        {
            if (catalogEntry == null) throw new ArgumentNullException("catalogEntry");

            var client = SearchClient.Instance;
            string language = catalogEntry.Language.Name;
            string mainCategory = catalogEntry.GetMainCategory(language);
            string category = catalogEntry.GetCategoryName(language);

            var result = client.Search<FindProduct>()
                .Filter(x => x.CategoryName.Match(category))
                .Filter(x => x.MainCategoryName.Match(mainCategory))
                .Filter(x => !x.Code.Match(catalogEntry.Code))
                .Filter(x => x.Language.Match(language))
                .StaticallyCacheFor(TimeSpan.FromMinutes(1))
                .Take(maxCount)
                .GetResult();

            List<ContentReference> links = new List<ContentReference>();
            foreach (FindProduct product in result)
            {
                links.Add(_referenceConverter.GetContentLink(product.Id, CatalogContentType.CatalogEntry, 0));
            }

            IRecommendations recommendations = new Recommendations("RecForProduct", _contentRepository.GetItems(links, catalogEntry.Language));
            return recommendations;
        }

        public IRecommendations GetRecommendedProducts(string userId, int maxCount, CultureInfo cultureInfo)
        {
            return null;
        }

        public IRecommendations GetRecommendedProductsByCategory(string userId, List<string> categories, int maxCount, CultureInfo cultureInfo)
        {
            return null;
        }

        public IRecommendations GetRecommendedProductsForCart(string userId, IEnumerable<string> productCodes, int maxCount, CultureInfo cultureInfo)
        {
            return null;
        }

        public Dictionary<string, double> GetScoreForItems(int maxCount = 10000)
        {
            return null;
        }
    }
}