using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.Recommendations.Tracking.Data;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Interfaces;
using OxxCommerceStarterKit.Web.EditorDescriptors;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.PageTypes;

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





    public interface IRecommendationsService
    {
        IEnumerable<Recommendation> GetRecommendationsForHomePage(HttpContextBase context);

        IEnumerable<Recommendation> GetRecommendationsForCategoryPage(NodeContent node, HttpContextBase context);

        IDictionary<string, IEnumerable<Recommendation>> GetRecommendationsForProductPage(string productCode, HttpContextBase context);

        IEnumerable<Recommendation> GetRecommendationsForBasketPage(HttpContextBase context);

        IEnumerable<Recommendation> GetRecommendationsForSearchPage(string term, IEnumerable<string> productCodes, HttpContextBase context);

        void TrackOrder(IPurchaseOrder purchaseOrder, HttpContextBase context);
    }

    [ServiceConfiguration(typeof(IRecommendationsService))]
    public class RecommendationsService : IRecommendationsService
    {
        private readonly TrackingDataFactory _trackingDataFactory;
        private readonly ITrackingService _trackingService;
        private readonly ReferenceConverter _referenceConverter;

        private readonly RecommendationsMode _mode;

        public RecommendationsService(TrackingDataFactory trackingDataFactory, ITrackingService trackingService, ReferenceConverter referenceConverter, IContentLoader contentLoader)
        {
            _trackingDataFactory = trackingDataFactory;
            _trackingService = trackingService;
            _referenceConverter = referenceConverter;

            var homePage = contentLoader.Get<HomePage>(ContentReference.StartPage);
            _mode = homePage.Settings.RecommendationsMode.ToEnum(RecommendationsMode.Disabled);
        }

        public IEnumerable<Recommendation> GetRecommendationsForHomePage(HttpContextBase context)
        {
            var trackingData = _trackingDataFactory.CreateHomeTrackingData(context);
            return GetRecommendations(trackingData, context)?.Values.FirstOrDefault();
        }

        public IEnumerable<Recommendation> GetRecommendationsForCategoryPage(NodeContent node, HttpContextBase context)
        {
            var trackingData = _trackingDataFactory.CreateCategoryTrackingData(node, context);
            return GetRecommendations(trackingData, context)?.Values.FirstOrDefault();
        }

        public IDictionary<string, IEnumerable<Recommendation>> GetRecommendationsForProductPage(string productCode, HttpContextBase context)
        {
            var trackingData = _trackingDataFactory.CreateProductTrackingData(productCode, context);
            return GetRecommendations(trackingData, context);
        }

        public IEnumerable<Recommendation> GetRecommendationsForBasketPage(HttpContextBase context)
        {
            var trackingData = _trackingDataFactory.CreateCartTrackingData(context);
            return GetRecommendations(trackingData, context)?.Values.FirstOrDefault();
        }

        public IEnumerable<Recommendation> GetRecommendationsForSearchPage(string term, IEnumerable<string> productCodes, HttpContextBase context)
        {
            var trackingData = _trackingDataFactory.CreateSearchTrackingData(term, productCodes, context);
            return GetRecommendations(trackingData, context)?.Values.FirstOrDefault();
        }

        public void TrackOrder(IPurchaseOrder purchaseOrder, HttpContextBase context)
        {
            var trackingData = _trackingDataFactory.CreateOrderTrackingData(purchaseOrder, context);
            _trackingService.Send(trackingData, context, RetrieveRecommendationMode.Disabled);
        }

        private IDictionary<string, IEnumerable<Recommendation>> GetRecommendations(TrackingDataBase trackingData, HttpContextBase context)
        {
            var returnValue = new Dictionary<string, IEnumerable<Recommendation>>();

            if (_mode == RecommendationsMode.Disabled)
                return returnValue;

            var result = _trackingService.Send(trackingData, context, RetrieveRecommendationMode.Enabled);

            if (_mode == RecommendationsMode.TrackingOnly && context.Request.QueryString["showrecs"] == null)
                return returnValue;

            if (result.SmartRecs != null)
            {
                foreach (var recommendation in result.SmartRecs)
                {
                    returnValue.Add(recommendation.Widget, recommendation.Recs.Select(x => new Recommendation(x.Id, _referenceConverter.GetContentLink(x.RefCode))));
                }
            }

            return returnValue;
        }
    }
}