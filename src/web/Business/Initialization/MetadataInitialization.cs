/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;
using OxxCommerceStarterKit.Core;

namespace OxxCommerceStarterKit.Web.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(CommerceInitialization))]
    public class MetadataInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            MetaDataContext mdContext = CatalogContext.MetaDataContext;

            var itemSizeKeyField = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order, Constants.Metadata.LineItem.Size, MetaDataType.ShortString, 255, true, false);
            JoinField(mdContext, itemSizeKeyField, Constants.Metadata.LineItem.ClassName);

            var itemUrl = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order,
                Constants.Metadata.LineItem.ImageUrl, MetaDataType.URL, 255, true, false);
            JoinField(mdContext, itemUrl, Constants.Metadata.LineItem.ClassName);

            //var description = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order,
            //    Constants.Metadata.LineItem.Description, MetaDataType.LongString, 255, true, false);
            //JoinField(mdContext, description, Constants.Metadata.LineItem.ClassName);

            var color = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order,
               Constants.Metadata.LineItem.Color, MetaDataType.ShortString, 255, true, false);
            JoinField(mdContext, color, Constants.Metadata.LineItem.ClassName);

            var colorImage = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order,
               Constants.Metadata.LineItem.ColorImageUrl, MetaDataType.URL, 255, true, false);
            JoinField(mdContext, colorImage, Constants.Metadata.LineItem.ClassName);

            var articleNumber = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order,
               Constants.Metadata.LineItem.ArticleNumber, MetaDataType.ShortString, 255, true, false);
            JoinField(mdContext, articleNumber, Constants.Metadata.LineItem.ClassName);

            MetaClass dibsPaymentClass = CreatePaymentMetaClass(mdContext, Constants.Metadata.Namespace.Order, "DibsPayment");

            var cardNumberMasked = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order, "CardNumberMasked",
                MetaDataType.ShortString, 255, true, false);
            JoinField(mdContext, cardNumberMasked, dibsPaymentClass.Name);

            var cardTypeName = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order, "CardTypeName",
               MetaDataType.ShortString, 255, true, false);
            JoinField(mdContext, cardTypeName, dibsPaymentClass.Name);

            var backEndOrderNumber = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order, Constants.Metadata.PurchaseOrder.BackendOrderNumber, MetaDataType.LongString, Int32.MaxValue, true, false);
            JoinField(mdContext, backEndOrderNumber, Constants.Metadata.PurchaseOrder.ClassName);

            var postNordTrackingId = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order, Constants.Metadata.PurchaseOrder.PostNordTrackingId, MetaDataType.LongString, Int32.MaxValue, true, false);
            JoinField(mdContext, postNordTrackingId, Constants.Metadata.PurchaseOrder.ClassName);

            var frequency = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order, Constants.Metadata.PurchaseOrder.Frequency, MetaDataType.ShortString, 255, true, false);
            JoinField(mdContext, frequency, Constants.Metadata.PurchaseOrder.ClassName);

            var latestDelivery = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order, Constants.Metadata.PurchaseOrder.LatestDelivery, MetaDataType.DateTime, 255, true, false);
            JoinField(mdContext, latestDelivery, Constants.Metadata.PurchaseOrder.ClassName);

            var deliveryServicePoint = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order, Constants.Metadata.Address.DeliveryServicePoint, MetaDataType.LongString, Int32.MaxValue, true, false);
            JoinField(mdContext, deliveryServicePoint, Constants.Metadata.Address.ClassName);

            var customerClub = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order, Constants.Metadata.OrderForm.CustomerClub, MetaDataType.Boolean, 1, true, false);
            JoinField(mdContext, customerClub, Constants.Metadata.OrderForm.ClassName);

            var selectedCategories = CreateMetaField(mdContext, Constants.Metadata.Namespace.Order, Constants.Metadata.OrderForm.SelectedCategories, MetaDataType.ShortString, 8000, true, false);
            JoinField(mdContext, selectedCategories, Constants.Metadata.OrderForm.ClassName);

        }

        private MetaClass CreatePaymentMetaClass(MetaDataContext mdContext, string metaClassNameSpace, string name)
        {

            string text1 = name;
            string text2 = name;
            string text3 = "Imported";
            MetaClass parentMetaClass = MetaClass.Load(mdContext, "OrderFormPayment");

            MetaClass metaClass = MetaClass.Load(mdContext, name);

            if (metaClass == null)
            {
                metaClass = MetaClass.Create(mdContext, metaClassNameSpace + ".User", text1, text2,
                    string.Format("{0}{1}{2}", (object)name, (object)"Ex_", (object)text1), parentMetaClass, false,
                    text3);
            }


            return metaClass;
        }

        public void Preload(string[] parameters) { }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }

        private MetaField CreateMetaField(MetaDataContext mdContext, string metaDataNamespace, string name, MetaDataType type, int length, bool allowNulls, bool cultureSpecific)
        {
            var f = MetaField.Load(mdContext, name) ??
                    MetaField.Create(mdContext, metaDataNamespace, name, name, string.Empty, type, length, allowNulls, cultureSpecific, false, false);
            return f;
        }

        private void JoinField(MetaDataContext mdContext, MetaField field, string metaClassName)
        {
            var cls = MetaClass.Load(mdContext, metaClassName);

            if (MetaFieldIsNotConnected(field, cls))
            {
                cls.AddField(field);
            }
        }

        private static bool MetaFieldIsNotConnected(MetaField field, MetaClass cls)
        {
            return cls != null && !cls.MetaFields.Contains(field);
        }
    }

}
