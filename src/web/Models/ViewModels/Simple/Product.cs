/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using OxxCommerceStarterKit.Web.Models.Catalog;

namespace OxxCommerceStarterKit.Web.Models.ViewModels.Simple
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductViewModel
    {
        string _url;
        string _imageUrl;
        bool _imageUrlIsSet;
        readonly FashionProductContent _product;
        IEnumerable<ProductVariantViewModel> _variants;
        Price _price;
        bool _priceIsSet;


        public string Name
        {
            get
            {
				return !string.IsNullOrEmpty(_product.DisplayName) ? _product.DisplayName : _product.Name;
            }
        }

        public string Url
        {
            get
            {
                return _url ?? (_url = ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(_product.ContentLink));
            }
        }

        public string ImageUrl
        {
            get
            {
                if (_imageUrlIsSet)
                    return _imageUrl;

                _imageUrlIsSet = true;

                return _imageUrl = _product.CommerceMediaCollection.
                    Where(m => m.GroupName == null || m.GroupName.ToUpperInvariant() != "SWATCH").
                    Select(m => ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(m.AssetLink)).
                    FirstOrDefault();
            }
        }

        

        public IEnumerable<ProductVariantViewModel> Variants
        {
            get
            {
                if (_variants == null)
                {
                    var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

                    _variants = contentLoader.GetChildren<VariationContent>(_product.ContentLink).
                        Concat(contentLoader.GetItems(
                            _product.GetVariantRelations(ServiceLocator.Current.GetInstance<IRelationRepository>()).Select(x => x.Target),
                            ServiceLocator.Current.GetInstance<LanguageSelector>()).OfType<VariationContent>()).
                        Where(e => e.IsAvailableInCurrentMarket(ServiceLocator.Current.GetInstance<ICurrentMarket>())).
                        Select(v => new ProductVariantViewModel(v));
                }

                return _variants;
            }
        }


        /// <summary>
        /// Gets the price of the product. Will loop through the variations
        /// and return the cheapest one.
        /// </summary>
        /// <remarks>
        /// If you know that all variations will always have the
        /// same price, you can optimize this and just pick the
        /// first variation with a price.
        /// Also note that this does not take into account if the
        /// cheapest variation is in stock
        /// </remarks>
        /// <value>
        /// The price of the cheapest varation
        /// </value>
        public Price Price
        {
            get
            {
				// TODO: Does not handle currently logged in user's customer price (club)
                if (_priceIsSet)
                    return _price;

                _priceIsSet = true;

                if (Variants.Count() == 0)
                    return _price = default(Price);

                foreach (var v in Variants)
                {
                    if (v.IsAvailable && v.Price != null)
                    {
                        // Pick the cheapest one
                        if (_price == null || _price.UnitPrice.Amount > v.Price.UnitPrice.Amount)
                            _price = v.Price;
                    }
                }

                return _price;
            }
        }
        

        public ProductViewModel(FashionProductContent product)
        {
            this._product = product;
        }
    }
}
