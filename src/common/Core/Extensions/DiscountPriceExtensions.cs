using EPiServer.Commerce.Marketing;

namespace OxxCommerceStarterKit.Core.Extensions
{
    public static class DiscountPriceExtensions
    {
        public static decimal GetDefaultPriceWithCheck(this DiscountPrice price)
        {
            return price != null ? price.DefaultPrice.Amount : 0;
        }
        public static decimal GetDiscountPriceWithCheck(this DiscountPrice price)
        {
            return price != null ? price.Price.Amount : 0;
        }

        public static string GetDefaultDisplayPriceWithCheck(this DiscountPrice price)
        {
            return price != null ? price.DefaultPrice.ToString() : string.Empty;
        }
        public static string GetDiscountDisplayPriceWithCheck(this DiscountPrice price)
        {
            return price != null ? price.Price.ToString() : string.Empty;
        }
    }
}