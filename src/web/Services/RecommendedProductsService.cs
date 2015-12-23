using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Interfaces;
using OxxCommerceStarterKit.Web.Api;
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

        public IEnumerable<IContent> GetRecommendedProducts(EntryContentBase catalogEntry, string userId, int maxCount)
        {
            if (catalogEntry == null) throw new ArgumentNullException(nameof(catalogEntry));

            var client = SearchClient.Instance;
            string language = catalogEntry.Language.Name;
            string mainCategory = catalogEntry.GetMainCategory(language);
            string category = catalogEntry.GetCategoryName(language);

            List<SimilarProductObject> similarProductObjects = new List<SimilarProductObject>();


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

            return _contentRepository.GetItems(links, catalogEntry.Language);
        }
    }
}