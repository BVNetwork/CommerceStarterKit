/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Pricing;
using OxxCommerceStarterKit.Core.Extensions;

namespace OxxCommerceStarterKit.Web.Helpers
{
    public static class PriceHelper
    {
        public static IEnumerable<IPriceValue> BuildValidPrices(string skuCode)
        {
            var priceService = ServiceLocator.Current.GetInstance<IPriceService>();

            IEnumerable<IPriceValue> entryPrices =
                priceService.GetCatalogEntryPrices(new CatalogKey(Mediachase.Commerce.Core.AppContext.Current.ApplicationId, skuCode))
                .Where(x => x.ValidUntil.GetNullableDateTime() >= DateTime.Now);

            return entryPrices;
        }
    }
}
