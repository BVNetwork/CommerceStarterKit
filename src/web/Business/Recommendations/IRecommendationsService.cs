using System.Collections.Generic;
using System.Web;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Recommendations.Commerce.Tracking;

namespace OxxCommerceStarterKit.Web.Business.Recommendations
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
}