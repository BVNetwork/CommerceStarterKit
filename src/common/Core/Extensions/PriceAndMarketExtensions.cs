using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxxCommerceStarterKit.Core.Models;

namespace OxxCommerceStarterKit.Core.Extensions
{
    public static class PriceAndMarketExtensions
    {
        public static decimal GetPriceWithCheck(this PriceAndMarket price)
        {
            return price != null ? price.UnitPrice.Amount : 0;
        }

        public static string GetDisplayPriceWithCheck(this PriceAndMarket price)
        {
            return price != null ? price.UnitPrice.ToString() : string.Empty;
        }
    }
}
