/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Extensions
{
    public static class PriceExtensions
    {
        private static Injected<ICurrentMarket> injectedMarketService;
    
        public static PriceModel GetPriceModel(this VariationContent currentContent)
        {
            return GetPriceModel(currentContent, injectedMarketService.Service.GetCurrentMarket());
        }
        public static PriceModel GetPriceModel(this VariationContent currentContent, IMarket market)
        {
            PriceModel priceModel = new PriceModel();
            priceModel.DefaultPrice = currentContent.GetPrice(market);
            priceModel.DiscountPrice = currentContent.GetDiscountPrice(market); 
            priceModel.CustomerClubPrice = currentContent.GetCustomerClubPrice(market);
            return priceModel;
        }
    }
}
