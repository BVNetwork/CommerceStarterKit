using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.ServiceLocation;
using EPiServer.Tracking.Commerce;
using EPiServer.Tracking.Commerce.Data;
using EPiServer.Tracking.Core;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Business.CustomTracking;
using OxxCommerceStarterKit.Web.EditorDescriptors;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.PageTypes;

namespace OxxCommerceStarterKit.Web.Business.Recommendations
{
    [ServiceConfiguration(typeof(IRecommendationService))]
    public class RecommendationService : IRecommendationService
    {
        private readonly TrackingDataFactory _trackingDataFactory;
        private readonly ITrackingService _trackingService;
        private readonly IProfileStoreService _profileStoreService;

        private readonly HomePage _homePage;
        private readonly RecommendationsMode _mode;

        public RecommendationService(TrackingDataFactory trackingDataFactory, ITrackingService trackingService, IContentLoader contentLoader, IProfileStoreService profileStoreService)
        {
            _trackingDataFactory = trackingDataFactory;
            _trackingService = trackingService;
            _profileStoreService = profileStoreService;

            _homePage = contentLoader.Get<HomePage>(ContentReference.StartPage);
            _mode = _homePage.Settings.RecommendationsMode.ToEnum(RecommendationsMode.Disabled);
        }

        public IEnumerable<Recommendation> GetRecommendationsForHomePage(HttpContextBase context, IContent content)
        {
            HomeTrackingData trackingData = _trackingDataFactory.CreateHomeTrackingData(context);
            return GetRecommendations(trackingData, context, _homePage)?.Values.FirstOrDefault();
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
            var categories = new List<string>();
            var trackingData = _trackingDataFactory.CreateOrderTrackingData(purchaseOrder, context);

            var items = new List<CartItemData>();

            foreach (var item in trackingData.Order.Items)
            {
                var sku = item.Variant?.Sku;
                
                var newItem = new CustomCartItemData(item.RefCode, sku, item.Quantity, item.Price);

                var product = GetProduct(item.RefCode);
                if (product != null)
                {
                    newItem.Categories = new List<string>();
                    foreach (var category in product.ParentCategoryName)
                    {
                        var name = category.Replace(" ", "");
                        var brand = product.Brand?.Replace(" ", "");
                        newItem.Categories.Add(name);
                        newItem.Categories.Add(name + "&amp;" + brand);
                    }

                    categories.AddRange(newItem.Categories);
                }

                items.Add(newItem);                
            }

            trackingData = new OrderTrackingData(items, trackingData.Order.Currency,trackingData.Order.Subtotal, trackingData.Order.Shipping, trackingData.Order.Total, trackingData.Order.OrderNo, trackingData.Lang, context, trackingData.User);

            _trackingService.Track(trackingData, context, content);

            try
            {
                _profileStoreService.UpdatePayload(categories, context);
            }
            catch (Exception ex)
            {

            }

        }

        private FindProduct GetProduct(string itemRefCode)
        {
            var result = SearchClient.Instance.Search<FindProduct>()
                .Filter(x => x.Language.Match("en"))
                .Filter(x => x.Code.MatchCaseInsensitive(itemRefCode))
                .GetResult();

            return result.FirstOrDefault();
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
                foreach (var recommendation in result.GetRecommendationGroups())
                {
                    returnValue.Add(recommendation.Area, recommendation.Recommendations);
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

    public class CustomCartItemData : CartItemData
    {
        public List<string> Categories { get; set; }

        public CustomCartItemData(string refCode, string variantCode, int quantity, Decimal price) : base(refCode,
            variantCode, quantity, price)
        {
        }
    }
}