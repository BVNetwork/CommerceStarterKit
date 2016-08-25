using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private readonly ITrackedRecommendationService _trackedRecommendationService;
        private readonly IRecommendationService _recommendationService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IContentRepository _contentRepository;

        public SannsynRecommendedProductsService(ITrackedRecommendationService trackedRecommendationService, IRecommendationService recommendationService, ReferenceConverter referenceConverter, IContentRepository contentRepository)
        {
            _trackedRecommendationService = trackedRecommendationService;
            _recommendationService = recommendationService;
            _referenceConverter = referenceConverter;
            _contentRepository = contentRepository;
        }

        public Interfaces.IRecommendations GetRecommendedProducts(EntryContentBase catalogEntry, string userId, int maxCount)
        {
            var recommendationsForProduct = _trackedRecommendationService.GetRecommendationsForProduct(catalogEntry.Code, maxCount);

            List<ContentReference> links = new List<ContentReference>();
            foreach (string code in recommendationsForProduct.ProductCodes)
            {
                links.Add(_referenceConverter.GetContentLink(code, CatalogContentType.CatalogEntry));
            }

            Interfaces.IRecommendations recommendations = new ProductRecommendations(recommendationsForProduct.RecommenderName, _contentRepository.GetItems(links, catalogEntry.Language));
            return recommendations;
        }

        public Interfaces.IRecommendations GetRecommendedProducts(string userId, int maxCount, CultureInfo cultureInfo)
        {
            var recommendationsForProduct = _trackedRecommendationService.GetRecommendationsForCustomer(userId, maxCount);

            List<ContentReference> links = new List<ContentReference>();
            foreach (string code in recommendationsForProduct.ProductCodes)
            {
                links.Add(_referenceConverter.GetContentLink(code, CatalogContentType.CatalogEntry));
            }

            Interfaces.IRecommendations recommendations = new ProductRecommendations(recommendationsForProduct.RecommenderName, _contentRepository.GetItems(links, cultureInfo));
            return recommendations;
        }

        public Interfaces.IRecommendations GetRecommendedProductsByCategory(string userId, List<string> categories, int maxCount, CultureInfo cultureInfo)
        {
            var recommendationsForProduct = _trackedRecommendationService.GetRecommendationsForCustomerByCategory(userId, categories, maxCount);

            List<ContentReference> links = new List<ContentReference>();
            foreach (string code in recommendationsForProduct.ProductCodes)
            {
                links.Add(_referenceConverter.GetContentLink(code, CatalogContentType.CatalogEntry));
            }

            Interfaces.IRecommendations recommendations = new ProductRecommendations(recommendationsForProduct.RecommenderName, _contentRepository.GetItems(links, cultureInfo));
            return recommendations;
        }

        public Interfaces.IRecommendations GetRecommendedProductsForCart(string userId, IEnumerable<string> productCodes, int maxCount, CultureInfo cultureInfo)
        {
            var recommendationsForCart = _trackedRecommendationService.GetRecommendationsForCart(userId, productCodes, maxCount);

            if(recommendationsForCart == null)
            {
                return null;
            }

            List<ContentReference> links = new List<ContentReference>();
            foreach (string code in recommendationsForCart.ProductCodes)
            {
                links.Add(_referenceConverter.GetContentLink(code, CatalogContentType.CatalogEntry));
            }

            Interfaces.IRecommendations recommendations = new ProductRecommendations(recommendationsForCart.RecommenderName, _contentRepository.GetItems(links, cultureInfo));
            return recommendations;
        }

        public Dictionary<string,double> GetScoreForItems(int maxCount = 10000)
        {
            return _recommendationService.GetScoreForItems(maxCount);
        }
    }
}