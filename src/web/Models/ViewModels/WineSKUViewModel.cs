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
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog.Objects;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Models.Catalog;
using Price = EPiServer.Commerce.SpecializedProperties.Price;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class WineSKUViewModel
    {
        public WineSKUContent CatalogContent { get; set; }
        public List<MediaData> Media { get; set; }
        public Price Price { get; set; }
        public CartItemModel CartItem { get; set; }

        public WineSKUViewModel(WineSKUContent currentContent)
        {
            CatalogContent = currentContent;
            Media = GetMedia(currentContent);
            CartItem = new CartItemModel(CatalogContent){CanBuyEntry = true};
        }
       
        private List<MediaData> GetMedia(WineSKUContent currentContent)
        {
            var contentLoader = ServiceLocator.Current.GetInstance<EPiServer.IContentLoader>();
            var mediaReferences = currentContent.AssetImageUrls();
            List<MediaData> mediaData = new List<MediaData>();
            foreach (ContentReference mediaReference in mediaReferences)
            {
                MediaData file;
                if (contentLoader.TryGet<MediaData>(mediaReference, out file))
                {
                    mediaData.Add(file);
                }
            }
            return mediaData;
        }

        public bool IsSellable { get; set; }
        public PriceModel PriceViewModel { get; set; }
    }
}
