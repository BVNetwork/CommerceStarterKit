/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Linq;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using OxxCommerceStarterKit.Core.Attributes;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;

namespace OxxCommerceStarterKit.Core.Objects
{
    public class Address : ICloneable
    {
        public Address(OrderAddressModel address)
        {
			if (address != null)
			{
				Id = address.Id.ToString();
				StreetAddress = address.Line1;
				ZipCode = address.PostalCode;
				City = address.City;
				FirstName = address.FirstName;
				LastName = address.LastName;
				CountryCode = address.CountryCode;
			}
        }

        public Address()
        {
            
        }

        public string Id { get; set; }
        
        public bool IsPreferredBillingAddress { get; set; }

        public bool IsPreferredShippingAddress { get; set; }

        [LocalizedDisplayName("/common/accountpages/address_label")]
        [Placeholder("/common/accountpages/placeholder/address")]
        public string StreetAddress { get; set; }

        [LocalizedDisplayName("/common/accountpages/zipcode_label")]
        [Placeholder("/common/accountpages/placeholder/zip")]
        public string ZipCode { get; set; }

        [LocalizedDisplayName("/common/accountpages/city_label")]
        [Placeholder("/common/accountpages/placeholder/city")]
        public string City { get; set; }

        [LocalizedDisplayName("/common/accountpages/firstname_label")]
        [Placeholder("/common/accountpages/placeholder/firstname")]
        public string FirstName { get; set; }

        [LocalizedDisplayName("/common/accountpages/lastname_label")]
        [Placeholder("/common/accountpages/placeholder/lastname")]
        public string LastName { get; set; }

		public string CountryCode { get; set; }

        public string DeliveryServicePoint { get; set; }

        public OrderAddress ToOrderAddress(string name)
        {
            var address = new OrderAddress();
            address.FirstName = FirstName;
            address.LastName = LastName;
            address.Name = name;
            address.Line1 = StreetAddress;
            address.PostalCode = ZipCode;
            address.City = City;
            
			// address[Constants.Metadata.Address.DeliveryServicePoint] = DeliveryServicePoint;
			address.CountryCode = CountryCode;
            return address;
        }

		public object Clone()
		{
			return this.MemberwiseClone();
		}


		public void CheckAndSetCountryCode()
		{
			if (string.IsNullOrEmpty(CountryCode))
			{
				var currentMarket = ServiceLocator.Current.GetInstance<ICurrentMarket>();
				var market = currentMarket.GetCurrentMarket();
				if (market != null && market.Countries.Any())
				{
					CountryCode = market.Countries.FirstOrDefault();
				}
			}
		}

	}
}
