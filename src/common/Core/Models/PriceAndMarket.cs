/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using Mediachase.Commerce;
using Mediachase.Commerce.Catalog.Objects;
using Newtonsoft.Json;

namespace OxxCommerceStarterKit.Core.Models
{
    public class PriceAndMarket
    {
        public string Price { get; set; }
        [JsonIgnore]
        public Money UnitPrice { get; set; }
        public string MarkedId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string PriceTypeId { get; set; }
        public string PriceCode { get; set; }

    }
}
