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
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Models;
using OxxCommerceStarterKit.Web.Models.Catalog;
using OxxCommerceStarterKit.Web.Models.Catalog.Base;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class ProductListViewModel
    {
        private UrlResolver _urlResolver;

        public ProductListViewModel()
        {
            _urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
        }

        public ProductListViewModel(VariationContent content, 
            IMarket currentMarket, 
            CustomerContact currentContact) : this()
        {
            ImageUrl = content.GetDefaultImage();
            AllImageUrls = content.AssetUrls();
            IsVariation = true;

            PopulateCommonData(content, currentMarket, currentContact);

            PopulatePrices(content, currentMarket);

        }

        public ProductListViewModel(ProductContent content, 
            IMarket currentMarket,
            CustomerContact currentContact)
            : this()
        {
            ImageUrl = content.GetDefaultImage();
            IsVariation = false;
            AllImageUrls = content.AssetUrls();

            PopulateCommonData(content, currentMarket, currentContact);


        }

        protected void PopulateCommonData(EntryContentBase content, IMarket currentMarket, CustomerContact currentContact)
        {
            Code = content.Code;
            ContentLink = content.ContentLink;
            DisplayName = content.DisplayName ?? content.Name;
            ProductUrl = _urlResolver.GetUrl(ContentLink);
            Description = content.GetPropertyValue("Description");
            Overview = content.GetPropertyValue("Overview");
            AverageRating = content.GetPropertyValue<double>("AverageRating");

            InStock = content.GetStock() > 0;

            ContentType = content.GetType().Name;

            if (string.IsNullOrEmpty(Overview))
                Overview = Description;

            CurrentContactIsCustomerClubMember = currentContact.IsCustomerClubMember();

        }

        protected void PopulatePrices(VariationContent content, IMarket currentMarket)
        {
            PriceString = content.GetDisplayPrice(currentMarket);
            PriceAmount = content.GetDefaultPriceAmount(currentMarket);

            var discountPriceAmount = content.GetDiscountPrice();
            DiscountPriceAmount = GetPriceWithCheck(discountPriceAmount);
            DiscountPriceString = GetDisplayPriceWithCheck(discountPriceAmount);

            DiscountPriceAvailable = DiscountPriceAmount > 0;

            var customerClubPriceAmount = content.GetCustomerClubPrice();
            CustomerClubMemberPriceAmount = GetPriceWithCheck(customerClubPriceAmount);
            CustomerClubMemberPriceString = GetDisplayPriceWithCheck(customerClubPriceAmount);

            CustomerPriceAvailable = CustomerClubMemberPriceAmount > 0;
        }

        

        private double GetPriceWithCheck(PriceAndMarket price)
        {
            return price != null ? (double)price.UnitPrice.Amount : 0;
        }

        private string GetDisplayPriceWithCheck(PriceAndMarket price)
        {
            return price != null ? price.UnitPrice.ToString() : string.Empty;
        }


        public string Code { get; set; }
        public string DisplayName { get; set; }
        public string NewItemText { get; set; }
        public string Description { get; set; }
        public ContentReference ContentLink { get; set; }
        
        // Pricing
        public string PriceString { get; set; }
        public double PriceAmount { get; set; }
        public string DiscountPriceString { get; set; }
        public double DiscountPriceAmount { get; set; }
        public string CustomerClubMemberPriceString { get; set; }
        public double CustomerClubMemberPriceAmount { get; set; }
        public bool CustomerPriceAvailable { get; set; }
        public bool DiscountPriceAvailable { get; set; }

        public string BrandName { get; set; }
        public Dictionary<string, ContentReference> Images { get; set; }
        public Dictionary<string, string> Variants { get; set; }
        public string Country { get; set; }
        public string ProductUrl { get; set; }
        public string ImageUrl { get; set; }
        public string ContentType { get; set; }
        public double AverageRating { get; set; }
        public List<string> AllImageUrls { get; set; }
        public string Overview { get; set; }
        public bool IsVariation { get; set; }
        public bool CurrentContactIsCustomerClubMember { get; set; }
        public bool InStock { get; set; }

    }
}
