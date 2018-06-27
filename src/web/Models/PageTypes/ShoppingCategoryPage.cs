/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using OxxCommerceStarterKit.Core.Attributes;
using OxxCommerceStarterKit.Web.Models.Blocks;

namespace OxxCommerceStarterKit.Web.Models.PageTypes
{
     [ContentType(GUID = "E346B4F1-DB39-404B-9980-D715B8F219AF",
                  DisplayName = "Product List Page",
                  Order = 100,
                  GroupName = WebGlobal.GroupNames.Commerce,
                  Description = "Shopping Category Page.")]
     [SiteImageUrl(thumbnail: EditorThumbnail.Commerce)]
    public class ShoppingCategoryPage : CommerceSampleModulePage, IHasFeatureProduct
    {
         [Searchable(true)]
         [Display(Name = "CatalogNodes", Description = "Selected Categories for this page", GroupName = SystemTabNames.Content, Order = 5)]
         [UIHint(UIHint.CatalogContent)]
         public virtual LinkItemCollection CatalogNodes { get; set; }

         [Searchable(false)]
         [Display(Name = "Number of products to show", Description = "", GroupName = SystemTabNames.Content, Order = 10)]
         public virtual int NumberOfProductsToShow { get; set; }

		 [Searchable]
		 [Display(Name = "TopContentArea", Description = "", GroupName = SystemTabNames.Content, Order = 20)]
		 [CultureSpecific]
		 public virtual ContentArea TopContentArea { get; set; }

		 [Searchable]
		 [Display(Name = "BottomContentArea", Description = "", GroupName = SystemTabNames.Content, Order = 30)]
		 [CultureSpecific]
		 public virtual ContentArea BottomContentArea { get; set; }

         [Display(Name = "Menu feature product", Description = "", GroupName = SystemTabNames.Content, Order = 40)]
         public virtual FeatureProductBlock FeatureProduct { get; set; }

        [Display(Name = "Hide recommendations", Description = "", GroupName = SystemTabNames.Content, Order = 50)]
        public virtual bool HideRecommendations { get; set; }
	}
}
