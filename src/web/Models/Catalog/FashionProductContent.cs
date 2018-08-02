/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using Microsoft.Ajax.Utilities;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Models;
using OxxCommerceStarterKit.Web.EditorDescriptors;
using OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories;
using OxxCommerceStarterKit.Web.Extensions;
using OxxCommerceStarterKit.Web.Models.Blocks.Contracts;
using OxxCommerceStarterKit.Web.Models.Catalog.Base;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Models.Catalog
{
    /// <summary>
    /// FashionProductContent class, maps to Fashion_Product_Class metadata class
    /// </summary>
    [CatalogContentType(DisplayName = "Fashion Product", 
        GUID = "18EA436F-3B3B-464E-A526-564E9AC454C7",
        MetaClassName = "Fashion_Product_Class")]
    public class FashionProductContent : ProductBase, IIndexableContent, IProductListViewModelInitializer
    {
        [CultureSpecific]
        [Display(Name = "Facet Color",
            Order = 10)]
        [Editable(true)]
        [SelectMany(SelectionFactoryType = typeof(ColorSelectionFactory))]
        public virtual string FacetColor { get; set; }

        [Display(Name = "Size Type",
           Order = 30)]
        public virtual string SizeType { get; set; }

        [Display(Name = "Size Unit", Order = 36)]
        public virtual string SizeUnit { get; set; }

        [Display(Name = "Fit",
           Order = 40)]
        [Editable(false)]
        public virtual string Fit { get; set; }

        [CultureSpecific]
        [Display(Name = "New item",
            Order = 55,
            Description = "Text describing new products (based on publish start date)")]
        public virtual string NewItemText { get; set; }

        [CultureSpecific]
        [Display(Name = "Size and fit",
            Order = 60)]
        public virtual XhtmlString SizeAndFit { get; set; }

        [CultureSpecific]
        [Display(Name = "Details and maintenance",
            Order = 70)]
        public virtual XhtmlString DetailsAndMaintenance { get; set; }


        [Display(Name = "Size guide", Order = 80)]
        // [AllowedTypes(new[] { typeof(ArticlePage) })]
        /// TODO: Convert to ContentReference
        public virtual Url SizeGuide { get; set; }


        public FindProduct GetFindProduct(IMarket market)
        {
            List<VariationContent> productVariants = GetVariants(this);
            var variations = GetFashionVariants(productVariants, market);

            var language = (Language == null ? string.Empty : Language.Name);
            string sizeType = string.IsNullOrEmpty(SizeType) ? "" : SizeType.First().ToString(CultureInfo.InvariantCulture);

            var findProduct = new FindProduct(this, language);

            findProduct.Description = Description;
            findProduct.Color = FacetColor == null ? new List<string>() : FacetColor.Split(',').ToList();
            findProduct.Sizes =
                variations.Select(x => x.Size == null ? string.Empty : x.Size.TrimEnd('-')).Distinct().ToList();
            findProduct.SizeType = sizeType;
            findProduct.SizeUnit = SizeUnit;
            findProduct.Fit = Fit;
            findProduct.SizesList = CreateSizeList(variations.Select(x => x.Size).Distinct().ToList(), SizeUnit,
                sizeType);

            findProduct.SetPriceData(productVariants, market);

            findProduct.Variants = variations;
            findProduct.NewItemText = NewItemText;
            return findProduct;
        }

        public List<VariationContent> GetVariants(ProductContent product)
        {
            var relationRepository = ServiceLocator.Current.GetInstance<IRelationRepository>();
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            CultureInfo cultureInfo = product.Language;

            IEnumerable<Relation> relationsBySource = relationRepository.GetRelationsBySource(product.ContentLink).OfType<ProductVariation>();
            List<VariationContent> productVariants = relationsBySource.Select(x => contentLoader.Get<VariationContent>(x.Target, new LanguageSelector(cultureInfo.Name))).ToList();
            return productVariants;
        }

        private List<FashionVariant> GetFashionVariants(List<VariationContent> productVariants, IMarket market)
        {
            List<FashionVariant> variations = new List<FashionVariant>();
            foreach (var variation in productVariants)
            {
                if (variation is FashionItemContent)
                {

                    FashionItemContent fashionItemContent = variation as FashionItemContent;
                    var fashinVariation = new FashionVariant()
                    {
                        Id = fashionItemContent.ContentLink.ID,
                        Color = fashionItemContent.FacetColor,
                        Size = fashionItemContent.Facet_Size != null ? fashionItemContent.Facet_Size.TrimEnd('-') : string.Empty,
                        Prices = fashionItemContent.GetPricesWithMarket(market),
                        Code = fashionItemContent.Code,
                        Stock = fashionItemContent.GetStock()
                    };
                    variations.Add(fashinVariation);
                }

            }
            return variations;
        }


        private List<string> CreateSizeList(List<string> sizes, string sizeUnit, string sizeType)
        {
            List<string> sizeList = new List<string>();
            foreach (var size in sizes)
            {
                if (!string.IsNullOrEmpty(size))
                    sizeList.Add(string.Format("{0}/{1}/{2}", sizeType, sizeUnit, size.TrimEnd('-')));
            }
            return sizeList;
        }


        public bool ShouldIndex()
        {
            return !(StopPublish != null && StopPublish < DateTime.Now);
        }

        public ProductListViewModel Populate(IMarket market)
        {
            UrlResolver urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();

            var variation = this.GetFirstVariation();

            ProductListViewModel productListViewModel = new ProductListViewModel(this, market, CustomerContext.Current.CurrentContact)
            {
                NewItemText = NewItemText,
                PriceString = variation.GetDisplayPrice(market),
            };
            ICurrentMarket currentMarket = ServiceLocator.Current.GetInstance<ICurrentMarket>();
            productListViewModel.PriceAmount = variation.GetDefaultPriceAmountWholeNumber(currentMarket.GetCurrentMarket());
            return productListViewModel;
        }
    }
}
