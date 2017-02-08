/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.ComponentModel.DataAnnotations;
using BVNetwork.EPiSendMail;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Web;
using OxxCommerceStarterKit.Core.Attributes;

namespace OxxCommerceStarterKit.Web.Models.PageTypes
{
	[ContentType(
		DisplayName = "Newsletter",
	   GUID = "4E783817-45BE-4270-A92F-17EC9F479F7B",
	   Description = "A page with a newsletter design.",
	   GroupName = WebGlobal.GroupNames.Default
       )]
    [SiteImageUrl(thumbnail: EditorThumbnail.Social)]
	public class NewsletterPage : NewsletterBase
	{

		[Display(
		  GroupName = SystemTabNames.Content,
		  Order = 130)]
		public virtual string Title { get; set; }
		
		[Display(
		  GroupName = SystemTabNames.Content,
		  Order = 135)]
		public virtual string Lead { get; set; }

		[UIHint(UIHint.MediaFile)]
		[Display(
		  GroupName = SystemTabNames.Content,
		  Order = 138)]
		public virtual ContentReference MainImage { get; set; }

		[Display(
		   GroupName = SystemTabNames.Content,
		   Order = 140)]
		public virtual XhtmlString MainBody { get; set; }

		[Display(
		  GroupName = SystemTabNames.Content,
		  Order = 150)]
		[UIHint(UIHint.Textarea)]
		public virtual string MainBodyText { get; set; }

		[Display(
			GroupName = SystemTabNames.Content,
			Order = 155)]
		public virtual XhtmlString Callout { get; set; }

		[Display(
			GroupName = SystemTabNames.Content,
			Order = 160)]
		public virtual ContentArea MainContentArea { get; set; }


	    [Display(
	        GroupName = SystemTabNames.Content,
	        Order = 69,
	        Name = "Utm Campaign Name")]
	    [CultureSpecific(false)]
	    public virtual string UtmCampaign { get; set; }

        [Display(Name = "Product List Title",
          GroupName = SystemTabNames.Content,
          Order = 168)]
        public virtual string MainProductListTitle { get; set; }

        [Display(Name = "Product List",
            GroupName = SystemTabNames.Content,
            Order = 170)]
        [AllowedTypes(typeof(EntryContentBase))]
        public virtual ContentArea MainProductList { get; set; }


    }

}
