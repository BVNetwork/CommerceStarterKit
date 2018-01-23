using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.ServiceLocation;
using EPiServer.Tracking.Commerce;
using EPiServer.Tracking.Commerce.Data;
using EPiServer.Tracking.Core;
using Mediachase.Commerce.Catalog;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.EditorDescriptors;
using OxxCommerceStarterKit.Web.Models.PageTypes;

namespace OxxCommerceStarterKit.Web.Business.Recommendations
{
    [ServiceConfiguration(typeof(IRecommendationService))]
    public class RecommendationService : IRecommendationService
    {
        private readonly TrackingDataFactory _trackingDataFactory;
        private readonly ITrackingService _trackingService;
        private readonly ReferenceConverter _referenceConverter;

        private readonly RecommendationsMode _mode;

        public RecommendationService(TrackingDataFactory trackingDataFactory, ITrackingService trackingService, ReferenceConverter referenceConverter, IContentLoader contentLoader)
        {
            _trackingDataFactory = trackingDataFactory;
            _trackingService = trackingService;
            _referenceConverter = referenceConverter;

            var homePage = contentLoader.Get<HomePage>(ContentReference.StartPage);
            _mode = homePage.Settings.RecommendationsMode.ToEnum(RecommendationsMode.Disabled);
        }

        public IEnumerable<Recommendation> GetRecommendationsForHomePage(HttpContextBase context, IContent content)
        {
            HomeTrackingData trackingData = _trackingDataFactory.CreateHomeTrackingData(context);
            return GetRecommendations(trackingData, context, content)?.Values.FirstOrDefault();
        }

        public IEnumerable<Recommendation> GetRecommendationsForCategoryPage(NodeContent node, HttpContextBase context, IContent content)
        {
            var trackingData = _trackingDataFactory.CreateCategoryTrackingData(node, context);
            return GetRecommendations(trackingData, context, content)?.Values.FirstOrDefault();
        }

        public IDictionary<string, IEnumerable<Recommendation>> GetRecommendationsForProductPage(string productCode, HttpContextBase context, IContent content)
        {
            var trackingData = _trackingDataFactory.CreateProductTrackingData(productCode, context);
            return GetRecommendations(trackingData, context, content);
        }

        public IEnumerable<Recommendation> GetRecommendationsForBasketPage(HttpContextBase context, IContent content)
        {
            var trackingData = _trackingDataFactory.CreateCartTrackingData(context);
            return GetRecommendations(trackingData, context, content)?.Values.FirstOrDefault();
        }

        public IEnumerable<Recommendation> GetRecommendationsForSearchPage(string term, IEnumerable<string> productCodes, HttpContextBase context, IContent content)
        {
            var trackingData = _trackingDataFactory.CreateSearchTrackingData(term, productCodes, context);
            return GetRecommendations(trackingData, context, content)?.Values.FirstOrDefault();
        }

        public void TrackOrder(IPurchaseOrder purchaseOrder, HttpContextBase context, IContent content)
        {
            var trackingData = _trackingDataFactory.CreateOrderTrackingData(purchaseOrder, context);
            _trackingService.Track(trackingData, context, content);
        }

        private IDictionary<string, IEnumerable<Recommendation>> GetRecommendations(CommerceTrackingData trackingData, HttpContextBase context, IContent content)
        {

            var returnValue = new Dictionary<string, IEnumerable<Recommendation>>();

            if (_mode == RecommendationsMode.Disabled || DoNotTrack(context))
                return returnValue;

            var result = _trackingService.Track(trackingData, context, content);

            if (result == null ||
                _mode == RecommendationsMode.TrackingOnly && context.Request.QueryString["showrecs"] == null)
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

        private bool DoNotTrack(HttpContextBase context)
        {
            if (context.Request.UserAgent.Contains("StatusCake"))
                return true;
            if (context.Request.UserAgent.Contains("CloudFlare-AlwaysOnline"))
                return true;

            
            return false;
        }
    }
}