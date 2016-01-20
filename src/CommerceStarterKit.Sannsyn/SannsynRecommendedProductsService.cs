using System.Collections.Generic;
using System.Globalization;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using Mediachase.Commerce.Catalog;
using OxxCommerceStarterKit.Interfaces;
using Sannsyn.Episerver.Commerce.Services;

namespace OxxCommerceStarterKit.Sannsyn
{
    public class SannsynRecommendedProductsService : IRecommendedProductsService
    {
        private readonly IRecommendationService _recommendationService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IContentRepository _contentRepository;

        public SannsynRecommendedProductsService(IRecommendationService recommendationService, ReferenceConverter referenceConverter, IContentRepository contentRepository)
        {
            _recommendationService = recommendationService;
            _referenceConverter = referenceConverter;
            _contentRepository = contentRepository;
        }

        public IEnumerable<IContent> GetRecommendedProducts(EntryContentBase catalogEntry, string userId, int maxCount)
        {
            var recommendationsForProduct = _recommendationService.GetRecommendationsForProduct(catalogEntry.Code, maxCount);

            List<ContentReference> links = new List<ContentReference>();
            foreach (string code in recommendationsForProduct)
            {
                links.Add(_referenceConverter.GetContentLink(code, CatalogContentType.CatalogEntry));
            }

            return _contentRepository.GetItems(links, catalogEntry.Language);


        }

        public IEnumerable<IContent> GetRecommendedProducts(string userId, int maxCount, CultureInfo cultureInfo)
        {
            var recommendationsForProduct = _recommendationService.GetRecommendationsForCustomer(userId, maxCount);

            List<ContentReference> links = new List<ContentReference>();
            foreach (string code in recommendationsForProduct)
            {
                links.Add(_referenceConverter.GetContentLink(code, CatalogContentType.CatalogEntry));
            }

            return _contentRepository.GetItems(links, cultureInfo);
        }

        public IEnumerable<IContent> GetRecommendedProductsByCagetory(string userId, List<string> categories, int maxCount, CultureInfo cultureInfo)
        {
            var recommendationsForProduct = _recommendationService.GetRecommendationsForCustomerByCategory(userId, categories, maxCount);

            List<ContentReference> links = new List<ContentReference>();
            foreach (string code in recommendationsForProduct)
            {
                links.Add(_referenceConverter.GetContentLink(code, CatalogContentType.CatalogEntry));
            }

            return _contentRepository.GetItems(links, cultureInfo);
        }

        public Dictionary<string,double> GetScoreForItems(int maxCount = 10000)
        {
            return _recommendationService.GetScoreForItems(maxCount);
        }
    }
}