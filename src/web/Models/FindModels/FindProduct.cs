/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Business.FacetRegistry;

namespace OxxCommerceStarterKit.Web.Models.FindModels
{
    public class FindProduct
    {
        private static Injected<UrlResolver> _urlResolver;

        public FindProduct()
        {
            
        }

        public FindProduct(EntryContentBase entryContentBase, string language)
        {
            IndexId = entryContentBase.ContentLink.ID + "_" + language;
            Id = entryContentBase.ContentLink.ID;
            Code = entryContentBase.Code;
            Name = entryContentBase.Name;
            DisplayName = entryContentBase.DisplayName;
            Language = language;
            Description = Description ?? null;
            Overview = Overview ?? null;
            ParentCategoryId = entryContentBase.GetProductCategoryIds(language);
            ParentCategoryName = entryContentBase.GetParentCategoryNames(language);
            MainCategoryName = entryContentBase.GetMainCategory(language);
            CategoryName = entryContentBase.GetCategoryName(language);
            ProductUrl = _urlResolver.Service.GetUrl(entryContentBase.ContentLink, language);
            DefaultImageUrl = entryContentBase.GetDefaultImage();
            AverageRating = entryContentBase.GetAverageRating();
            DefaultInventory = entryContentBase.GetStock();
        }      

        [Id]
        public string IndexId { get; set; }
        
        public int Id { get; set; }
        
        public string Code { get; set; }
        
        public string Name { get; set; }
        
        [StringFilter(DisplayName = "Display Name")]
        public string DisplayName { get; set; }
        
        [StringFilter]
        public string Language { get; set; }
        
        public XhtmlString Description { get; set; }
        
        public XhtmlString Overview { get; set; }
        
        public List<string> Color { get; set; }
        
        public List<string> Sizes { get; set; }
        
        [StringFilter]
        public string SizeUnit { get; set; }
        
        [StringFilter]
        public string SizeType { get; set; }
        
        [StringFilter]
        public string Fit { get; set; }
        public List<string> SizesList { get; set; } 
        public List<string> ParentCategoryName { get; set; }
        public List<int> ParentCategoryId { get; set; }
        [StringFilter(DisplayName = "Main Category")]
        public string MainCategoryName { get; set; }
        [StringFilter(DisplayName = "Category")]
        public string CategoryName { get; set; }
        public string DefaultImageUrl { get; set; }
        public string ProductUrl { get; set; }
        [NumericFilter(DisplayName = "Default Inventory")]
        public decimal DefaultInventory { get; set; }

        /// <summary>
        /// Prices - used for filtering on range, is rounded to int
        /// </summary>
        public string DefaultPrice { get; set; }
        [NumericFilter(DisplayName = "Defalt Price")]
        public int DefaultPriceAmount { get; set; }

        public string DiscountedPrice { get; set; }
        [NumericFilter(DisplayName = "Discounted Price")]
        public double DiscountedPriceAmount { get; set; }
        
        public string CustomerClubPrice { get; set; }
        [NumericFilter(DisplayName = "Customer Club Price")]
        public double CustomerClubPriceAmount { get; set; }

        public List<FashionVariant> Variants { get; set; }
        public List<GenericFindVariant> GenericVariants { get; set; }
		public string NewItemText { get; set; }

        public int SalesCounter { get; set; }

        [StringFilter(DisplayName = "Brand")]
        public string Brand { get; set; }

        [StringFilter]
        public string Country { get; set; }
        
        //Photo facets
        [StringFilter]
        public decimal Resolution { get; set; }
        
        [StringFilter(DisplayName = "Lens Mount")]
        public string LensMount { get; set; }
        
        [StringFilter(DisplayName = "Camera Format")]
        public string CameraFormat { get; set; }
        
        [StringFilter(DisplayName = "File Format")]
        public string FileFormat { get; set; }
        
        [StringFilter]
        public string Connectivity { get; set; }
        
        [StringFilter]
        public string Battery { get; set; }
        
        [StringFilter(DisplayName = "Memory Card Type")]
        public string MemoryCardType { get; set; }
        
        [NumericFilter]
        public double Weight { get; set; }
        
        [NumericFilter(DisplayName = "Average Rating")]
        public double AverageRating { get; set; }

        public double Score { get; set; }
    }
}
