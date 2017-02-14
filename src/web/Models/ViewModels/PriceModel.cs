/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.SpecializedProperties;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class PriceModel
	{
		public Price DefaultPrice { get; set; }
		public DiscountPrice DiscountPrice { get; set; }
        public string CustomerClubDisplayPrice { get; set; }
        public Price CustomerClubPrice { get; set; }


		public PriceModel()
		{

		}

        public bool HasDiscount()
        {
            return DiscountPrice != null;
        }
        public bool HasCustomerPrice()
        {
            return CustomerClubPrice != null;
        }

		public PriceModel(Price price)
			: this()
		{
			if (price != null)
			{
				DefaultPrice = price;
			}
			else
			{
				DefaultPrice = default(Price);
			}
		}

	}
}
