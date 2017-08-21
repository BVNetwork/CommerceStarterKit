using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.Recommendations.Tracking.Data;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.EditorDescriptors;
using OxxCommerceStarterKit.Web.Models.PageTypes;

namespace OxxCommerceStarterKit.Web.Services
{
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