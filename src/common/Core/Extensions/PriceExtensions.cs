/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.ServiceLocation;
using log4net.Util;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce.Website.Helpers;
using OxxCommerceStarterKit.Core.Models;
using Price = EPiServer.Commerce.SpecializedProperties.Price;

namespace OxxCommerceStarterKit.Core.Extensions
{
    public static class PriceExtensions
    {
        private static Injected<ReadOnlyPricingLoader> injectedPriceLoader;
        private static Injected<IPromotionEngine> _promotionEngine;
        private static Injected<ICurrentMarket> _currentMarket;
        

        public static DiscountPrice GetDiscountPrice(this VariationContent currentContent, IMarket market)
        {
            try
            {
                IMarket currentMarket = market;
                if (market == null)
                    currentMarket = _currentMarket.Service.GetCurrentMarket();

                var discountedEntries = _promotionEngine.Service.GetDiscountPrices(currentContent.ContentLink,
                    currentMarket);
                if (discountedEntries.Any())
                {
                    return discountedEntries.First().DiscountPrices.Last();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets price information for a variation as a Price object. You can get the monetary price from the UnitPrice member.
        /// </summary>
        /// <param name="pricing">The variation.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public static Price GetPrice(this IPricing pricing, IMarket market)
        {
            if(pricing == null)
            {
                return null;
            }

            IMarket currentMarket = market;
            if (market == null)
                currentMarket = _currentMarket.Service.GetCurrentMarket();

            return pricing.GetPrices(injectedPriceLoader.Service).FirstOrDefault(x => x.MarketId == currentMarket.MarketId);
        }

        public static Price GetDefaultPrice(this List<VariationContent> variations, IMarket market = null)
        {
            if (variations.Any())
            {
                foreach (var variation in variations)
                {
                    var price = variation.GetPrice(market);
                    if (price != null)
                    {
                        return price;
                    }
                }
            }
            return null;
        }

        public static int GetDefaultPriceAmountWholeNumber(this VariationContent variation, IMarket market)
        {
            Price price = variation.GetPrice(market);
            return price != null ? decimal.ToInt32(price.UnitPrice.Amount) : 0;
        }

        public static int GetDefaultPriceAmountWholeNumber(this List<VariationContent> variations, IMarket market)
        {
            Price price = variations.GetDefaultPrice(market);
            return price != null ? decimal.ToInt32(price.UnitPrice.Amount) : 0;
        }

        /// <summary>
        /// Gets the display price for the variation and market, including currency symbol.
        /// </summary>
        /// <param name="variation">The variation to retrieve price from.</param>
        /// <param name="market">The market to get price for. If null, the current market is used.</param>
        /// <returns></returns>
        public static string GetDisplayPrice(this VariationContent variation, IMarket market = null)
        {
            Price price = variation.GetPrice(market);
            return price != null ? price.UnitPrice.ToString() : string.Empty;
        }

        public static Price GetCustomerClubPrice(this VariationContent variation, IMarket market)
        {
            var prices = variation.GetPrices();

            Func<Price, bool> priceFilter = delegate(Price d)
            {
                return d.CustomerPricing.PriceCode != string.Empty &&
                       (d.CustomerPricing.PriceTypeId == CustomerPricing.PriceType.PriceGroup &&
                        d.CustomerPricing.PriceCode == Constants.CustomerGroup.CustomerClub);
            };

            if (prices.Any())
            {
                var price = prices.FirstOrDefault(priceFilter);
                return price;
            }

            return null;
        }

        public static decimal GetPriceAmountSafe(this Price price)
        {
            if(price == null)
            {
                return 0;
            }
            return price.UnitPrice.Amount;
        }
        public static string GetPriceAmountStringSafe(this Price price)
        {
            if (price == null)
            {
                return string.Empty;
            }
            return price.UnitPrice.ToString();
        }
    }

}
