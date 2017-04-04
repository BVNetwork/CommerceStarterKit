/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public interface IVariationViewModel<out TVariationContent> : IBaseCatalogViewModel<TVariationContent>
        where TVariationContent : VariationContent        
    {
        Lazy<Inventory> Inventory { get; set; }
        Price Price { get; set; }
        EntryContentBase ParentEntry { get; set; }
        EntryContentBase ContentWithAssets { get; set; }


        PriceModel PriceViewModel { get; set; }
        IEnumerable<IVariationViewModel<VariationContent>> AllVariationSameStyle { get; set; }
        bool IsSellable { get; set; }
        List<MediaData> Media { get; set; }
        CartItemModel CartItem { get; set; }
        ContentArea RelatedProductsContentArea { get; set; }
        List<ProductListViewModel> ProductAlternatives { get; set; }
    }
}
