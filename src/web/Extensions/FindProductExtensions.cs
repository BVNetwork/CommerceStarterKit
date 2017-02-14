using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using Mediachase.Commerce;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Models;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Extensions
{
    public static class FindProductExtensions
    {
        public static void SetPriceData(this FindProduct findProduct, VariationContent content, IMarket market)
        {
            PriceModel priceModel = content.GetPriceModel(market);
            findProduct.DefaultPrice = priceModel.DefaultPrice.UnitPrice.ToString();
            findProduct.DefaultPriceAmount = content.GetDefaultPriceAmountWholeNumber(market);

            DiscountPrice discountPrice = priceModel.DiscountPrice;
            findProduct.DiscountedPriceAmount = (double)discountPrice.GetDiscountPriceWithCheck();
            findProduct.DiscountedPrice = discountPrice.GetDiscountDisplayPriceWithCheck();

            findProduct.CustomerClubPriceAmount = (double) priceModel.CustomerClubPrice.GetPriceAmountSafe(); 
            findProduct.CustomerClubPrice = priceModel.CustomerClubPrice.GetPriceAmountStringSafe();
        }

        /// <summary>
        /// Sets discount price for the first variation with a price
        /// </summary>
        /// <param name="findProduct"></param>
        /// <param name="content"></param>
        /// <param name="market"></param>
        public static void SetPriceData(this FindProduct findProduct, List<VariationContent> content, IMarket market)
        {
            VariationContent variation = null;
            if (content.Any())
            {
                foreach (var item in content)
                {
                    var price = item.GetPrice(market);
                    if (price != null)
                    {
                        variation = item;
                        break;
                    }
                }
            }

            if(variation == null)
            {
                return;
            }

            PriceModel priceModel = variation.GetPriceModel(market);
            findProduct.DefaultPrice = priceModel.DefaultPrice.UnitPrice.ToString();
            findProduct.DefaultPriceAmount = content.GetDefaultPriceAmountWholeNumber(market);

            DiscountPrice discountPrice = priceModel.DiscountPrice;
            findProduct.DiscountedPriceAmount = (double)discountPrice.GetDiscountPriceWithCheck();
            findProduct.DiscountedPrice = discountPrice.GetDiscountDisplayPriceWithCheck();

            findProduct.CustomerClubPriceAmount = (double)priceModel.CustomerClubPrice.GetPriceAmountSafe();
            findProduct.CustomerClubPrice = priceModel.CustomerClubPrice.GetPriceAmountStringSafe();
        }
    }
}