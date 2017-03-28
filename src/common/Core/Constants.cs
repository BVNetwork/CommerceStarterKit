/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

namespace OxxCommerceStarterKit.Core
{
    public static class Constants
    {
        public static class AppSettings
        {
            public const string ResetPasswordExpireKey = "ResetPasswordExpireKey";
        }

		public static class CustomerGroup
		{
			public static string CustomerClub = "CustomerClub";
		}

        public static class Metadata
        {
            public static class Namespace
            {
                public const string Order = "Mediachase.Commerce.Orders";
                public const string Catalog = "Mediachase.Commerce.Catalog";
            }

            public class LineItem
            {
                public const string Size = "ItemSize";
                public const string ClassName = "LineItemEx";
                public const string ImageUrl = "ImageUrl";
                public const string Description = "Description";
                public const string Color = "Color";
                public const string ColorImageUrl = "ColorImageUrl";
                public const string ArticleNumber = "ArticleNumber";
                public const string Quantity = "Quantity";
                public const string DisplayName = "DisplayName";
            }

            public static class Customer
            {
                public const string PhoneNumber = "PhoneNumber";
				public const string Category = "Category";
				public const string HasPassword = "HasPassword";
            }

			public static class OrderForm
			{
				public const string ClassName = "OrderFormEx";
				public const string CustomerClub = "MemberClub";
				public const string SelectedCategories = "SelectedCategories";
			}

			public static class PurchaseOrder
			{
                public const string BackendOrderNumber = "BackendOrderNumber";
                public const string PostNordTrackingId = "PostNordTrackingId";
				public const string ClassName = "PurchaseOrder";
			}

			public static class Address
			{
				public const string ClassName = "OrderAddressEx";
				public const string DeliveryServicePoint = "DeliveryServicePoint";
			}
        }

        public static class ViewData
        {
            public const string Angular = "angular";
            public const string PlaceholderData = "PlaceholderData";
        }

        public class UIHint
        {
            public const string PaymentMethod = "paymentmethod";
			public const string HotSpotsEditor = "hotspotseditor";
        }

        public class Order
        {
            public const string ShippingAddressName = "Shipping";
            public const string BillingAddressName = "Billing";

            public static class Cartname
            {
                public const string Quickbuy = "Quickbuy";
            }
        }

        public class AssociationTypes
        {
            public const string Default = "Default";
            // Not in use - default is the one to use
			public const string RecommendedProducts = "RecommendedProducts";
	        public const string SameStyle = "SameStyle";
            public const string Accessory = "Accessory";
            public const string CrossSell = "CrossSell";
            public const string Upsell = "Upsell";
            public const string Replacement = "Replacement";
        }

        public class CodePrefix
        {
            public const string Node = "node";
            public const string Category = "category";
        }

        public class Warehouse
        {
            public const string DefaultWarehouseCode = "default";
        }
    }
}
