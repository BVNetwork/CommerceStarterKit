/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using OxxCommerceStarterKit.Web.EditorDescriptors;
using OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories;

namespace OxxCommerceStarterKit.Web.Models.PageTypes
{
	[ContentType(
		GUID = "75EA193A-DDBF-4590-8E22-BF74ABE56906",
		AvailableInEditMode = false,        // Just for settings, not able to add from edit mode
		DisplayName = "Settings Data",
		Description = "Contains global settings data for this site.",
		GroupName = "Commerce System",
		Order = 1)]
	public class SettingsBlock : BlockData
	{
		[Searchable(false)]
		[Display(
			Name = "CartPage",
			Description = "The page that displays the shopping cart.",
			GroupName = SystemTabNames.Settings,
			Order = 10)]
		[CultureSpecific]
		public virtual PageReference CartPage { get; set; }


		[Searchable(false)]
		[Display(
			Name = "WishList Page",
			Description = "The page that displays the wish list.",
			GroupName = SystemTabNames.Settings,
			Order = 20)]
		[CultureSpecific]
		public virtual PageReference WishListPage { get; set; }

		[Searchable(false)]
		[Display(
			Name = "Checkout Page",
			Description = "The checkout page to complete your order.",
			GroupName = SystemTabNames.Settings,
			Order = 30)]
		[CultureSpecific]
		public virtual PageReference CheckoutPage { get; set; }

        [Searchable(false)]
        [Display(
            Name = "Payment Container",
            Description = "The container page for different payment methods.",
            GroupName = SystemTabNames.Settings,
            Order = 33)]
        [CultureSpecific]
        public virtual PageReference PaymentContainerPage { get; set; }

		[Searchable(false)]
		[Display(
			Name = "Account Page",
			Description = "The page that displays your account information.",
			GroupName = SystemTabNames.Settings,
			Order = 35)]
		[CultureSpecific]
		public virtual PageReference AccountPage { get; set; }

		[Searchable(false)]
		[Display(
			Name = "Your Orders Page",
			Description = "The page that displays the order history.",
			GroupName = SystemTabNames.Settings,
			Order = 40)]
		public virtual PageReference YourOrdersPage { get; set; }

		[Searchable(false)]
		[Display(
			Name = "Receipt Page",
			Description = "The page you see after your order is complete",
			GroupName = SystemTabNames.Settings,
			Order = 45)]
		[CultureSpecific]
		public virtual PageReference ReceiptPage { get; set; }

		[Searchable(false)]
		[Display(
			Name = "Newsletter unsubscribe page",
			Description = "The page where the user can unsubscribe from the newsletter",
			GroupName = SystemTabNames.Settings,
			Order = 47)]
		public virtual PageReference NewsletterUnsubscribePage { get; set; }

		[Searchable(false)]
		[CultureSpecific]
		[Display(Name = "Delivery And Returns",
            Description = "Text shown in popup",
			GroupName=SystemTabNames.Content,
			Order = 90)]
		public virtual XhtmlString DeliveryAndReturns { get; set; }

		[Searchable(false)]
		[Display(Name = "Login Page",
			GroupName = SystemTabNames.Settings,
			Order=90)]
		public virtual ContentReference LoginPage { get; set; }

	    [Searchable(false)]
		[Display(Name = "Search Page",
			GroupName = SystemTabNames.Settings,
			Order = 110)]
		public virtual ContentReference SearchPage { get; set; }

		[Searchable(false)]
		[Display(Name = "Error 500 title",
			GroupName = SystemTabNames.Settings,
			Order = 130)]
		// [CultureSpecific] - NOTE! You do not know what language caused an error
		public virtual string ErrorPageTitle { get; set; }

		[Searchable(false)]
		[Display(Name = "Error 500 text",
			GroupName = SystemTabNames.Settings,
			Order = 140)]
		// [CultureSpecific] - NOTE! You do not know what language caused an error
		public virtual XhtmlString ErrorPageText { get; set; }

		[Searchable(false)]
		[Display(Name = "Error 404 title",
			GroupName = SystemTabNames.Settings,
			Order = 150)]
		// [CultureSpecific] - NOTE! You do not know what language caused an error
		public virtual string FileNotFoundPageTitle { get; set; }

		[Searchable(false)]
		[Display(Name = "Error 404 text",
			GroupName = SystemTabNames.Settings,
			Order = 160)]
		// [CultureSpecific] - NOTE! You do not know what language caused an error
		public virtual XhtmlString FileNotFoundPageText { get; set; }

		[Searchable(false)]
        [CultureSpecific]
		[Display(Name = "Javascripts here will be added to header tag",
			GroupName = SystemTabNames.Settings,
			Order = 200)]
        [UIHint(UIHint.Textarea)]
		public virtual string HeaderScripts { get; set; }

	    [Display(
	        GroupName = SystemTabNames.Settings,
	        Order = 220,
            Name = "Release Shipment Automatically",
            Description = "If selected, an order shipment will automatically be released, skipping the pick list.")]
	    [CultureSpecific(false)]
	    public virtual  bool ReleaseShipmentAutomatically { get; set; }

        [Searchable(false)]
        [CultureSpecific]
        [Display(Name = "Color scheme",
            GroupName = SystemTabNames.Settings,
            Order = 240)]
        [SelectOne(SelectionFactoryType = typeof(ColorSchemeSelectionFactory))]
        public virtual string Scheme { get; set; }

        [Display(Order = 250, GroupName = SystemTabNames.Settings)]
        [SelectOne(SelectionFactoryType = typeof(RecommendationsModeSelectionFactory))]
        public virtual string RecommendationsMode { get; set; }

    }
}
