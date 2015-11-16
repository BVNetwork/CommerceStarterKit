/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using Mediachase.Commerce.Orders;
using OxxCommerceStarterKit.Core.Extensions;

namespace OxxCommerceStarterKit.Core.Objects
{
    public class LineItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Code { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public string ArticleNumber { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public string ColorImageUrl { get; set; }        
        public decimal PlacedPrice { get; set; }
        public decimal LineItemTotal { get; set; }
        public decimal LineItemDiscount { get; set; }
        public decimal LineItemOrderLevelDiscount { get; set; }
		public List<DiscountItem> Discounts { get; set; }
        public bool IsInventoryAllocated { get; set; }

        public LineItem() { }
		public LineItem(Mediachase.Commerce.Orders.LineItem lineItem, string language)
		{
            Code = lineItem.Code;
			Name = lineItem.GetStringValue(Constants.Metadata.LineItem.DisplayName);
			ArticleNumber = lineItem.GetStringValue(Constants.Metadata.LineItem.ArticleNumber);
			ImageUrl = lineItem.GetString(Constants.Metadata.LineItem.ImageUrl);
			Color = lineItem.GetStringValue(Constants.Metadata.LineItem.Color);
			ColorImageUrl = lineItem.GetStringValue(Constants.Metadata.LineItem.ColorImageUrl);
			Description = lineItem.GetStringValue(Constants.Metadata.LineItem.Description);
			Size = lineItem.GetStringValue(Constants.Metadata.LineItem.Size);
			PlacedPrice = lineItem.PlacedPrice;
			LineItemTotal = lineItem.Quantity * lineItem.PlacedPrice;
			LineItemDiscount = lineItem.LineItemDiscountAmount;
			LineItemOrderLevelDiscount = lineItem.OrderLevelDiscountAmount;
			Quantity = Convert.ToInt32(lineItem.Quantity);
			Url = lineItem.GetEntryLink(language);
			Discounts = lineItem.Discounts.Cast<Discount>().Select(x => new DiscountItem(x)).ToList();
            IsInventoryAllocated = lineItem.IsInventoryAllocated;
		}
    }
}
