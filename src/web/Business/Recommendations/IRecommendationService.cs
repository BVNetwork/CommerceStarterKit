using System.Collections.Generic;
using System.Web;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Personalization.Commerce.Tracking;

namespace OxxCommerceStarterKit.Web.Business.Recommendations
{
    public interface IRecommendationService
    {
        IEnumerable<Recommendation> GetRecommendationsForHomePage(HttpContextBase context, IContent content);

        IEnumerable<Recommendation> GetRecommendationsForCategoryPage(NodeContent node, HttpContextBase context, IContent content);

        IDictionary<string, IEnumerable<Recommendation>> GetRecommendationsForProductPage(string productCode, HttpContextBase context, IContent content);

        IEnumerable<Recommendation> GetRecommendationsForBasketPage(HttpContextBase context, IContent content);

        IEnumerable<Recommendation> GetRecommendationsForSearchPage(string term, IEnumerable<string> productCodes, HttpContextBase context, IContent content);

        void TrackOrder(IPurchaseOrder purchaseOrder, HttpContextBase context, IContent content);
    }
}