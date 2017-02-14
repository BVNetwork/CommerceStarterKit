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
using OxxCommerceStarterKit.Web.Extensions;
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
            
            var variation = content.GetFirstVariation();
            if (variation != null)
            {
                PopulatePrices(variation, currentMarket);
            }
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
            var priceModel = content.GetPriceModel(currentMarket);

            PriceString = priceModel.DefaultPrice.UnitPrice.ToString();
            PriceAmount = priceModel.DefaultPrice.UnitPrice.Amount;

            DiscountPriceAmount = priceModel.HasDiscount() ? priceModel.DiscountPrice.Price.Amount : 0;
            DiscountPriceString = priceModel.HasDiscount() ? priceModel.DiscountPrice.Price.ToString() : "";

            DiscountPriceAvailable = DiscountPriceAmount > 0;

            CustomerClubMemberPriceAmount = priceModel.CustomerClubPrice.GetPriceAmountSafe();
            CustomerClubMemberPriceString = priceModel.CustomerClubPrice.GetPriceAmountStringSafe();

            CustomerPriceAvailable = CustomerClubMemberPriceAmount > 0;
        }


        public string GetTrackingName()
        {
            if (string.IsNullOrEmpty(TrackingName) == false)
            {
                return TrackingName + "_" + Code;
            }
            return string.Empty;
        }

        public string Code { get; set; }
        public string DisplayName { get; set; }
        public string NewItemText { get; set; }
        public string Description { get; set; }
        public ContentReference ContentLink { get; set; }

        // Pricing
        public string PriceString { get; set; }
        public decimal PriceAmount { get; set; }
        public string DiscountPriceString { get; set; }
        public decimal DiscountPriceAmount { get; set; }
        public string CustomerClubMemberPriceString { get; set; }
        public decimal CustomerClubMemberPriceAmount { get; set; }
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
        public string TrackingName { get; set; }

    }
}
