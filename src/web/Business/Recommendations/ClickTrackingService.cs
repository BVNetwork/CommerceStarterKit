using System.Web;
using EPiServer.Business.Commerce;
using EPiServer.Recommendations.Commerce.Tracking;

namespace OxxCommerceStarterKit.Web.Business.Recommendations
{
    public class ClickTrackingService : IClickTrackingService
    {
        /// <summary>
        /// Gets the id of the recommendation that was clicked to initiate the current request.
        /// </summary>
        /// <param name="context">The current http context.</param>
        /// <returns>The recommendation id, or 0 if the current request was not initiated by clicking on a recommendation.</returns>
        public long GetRecommendationId(HttpContextBase context)
        {
            var recommendationIdKey = "RecommendationId";
            if (context.Items.Contains(recommendationIdKey))
            {
                return (long)context.Items[recommendationIdKey];
            }

            long recommendationId;
            long.TryParse(CookieHelper.Get(recommendationIdKey), out recommendationId);
            context.Items.Add(recommendationIdKey, recommendationId);

            CookieHelper.Remove(recommendationIdKey);

            return recommendationId;
        }
    }
}