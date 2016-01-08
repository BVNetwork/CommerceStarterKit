using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Models;
using OxxCommerceStarterKit.Web.Models.Blocks.Contracts;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Models.Catalog
{
    [CatalogContentType(DisplayName = "Generic size variation",
      GUID = "6C00EADF-9246-42FF-8833-CB5FEA79B1C7",
      MetaClassName = "GenericSizeVariationContent")]
    public class GenericSizeVariationContent : VariationContent, IIndexableContent, IProductListViewModelInitializer, IResourceable
    {
        // Same for all languages
        [Display(Name = "Facet Brand",
            Order = 15)]
        public virtual string Facet_Brand { get; set; }

        [Display(Name = "Size", Order = 20)]
        public virtual string Size { get; set; }

        [Display(Name = "Color", Order = 30)]
        [CultureSpecific]
        public virtual string Color { get; set; }

        [Display(
        GroupName = SystemTabNames.Content,
        Order = 80,
        Name = "Description")]
        [CultureSpecific]
        public virtual XhtmlString Description { get; set; }

        // Multi lang
        [Display(Name = "Overview", Order = 100)]
        [CultureSpecific]
        public virtual XhtmlString Overview { get; set; }

        // Multi lang
        [Display(Name = "Details", Order = 120)]
        [CultureSpecific]
        public virtual XhtmlString Details { get; set; }

        [Display(
           GroupName = SystemTabNames.Content,
           Order = 300,
           Name = "Average Rating")]
        [Editable(false)]
        public virtual double AverageRating { get; set; }

        public FindProduct GetFindProduct(Mediachase.Commerce.IMarket market)
        {
            var language = (Language == null ? string.Empty : Language.Name);
            var findProduct = new FindProduct(this,language);
            findProduct.Color = Color != null ? new List<string>() { Color } : new List<string>();
            findProduct.Description = Description;
            findProduct.Overview = Overview;
            EPiServer.Commerce.SpecializedProperties.Price defaultPrice = this.GetDefaultPrice();
            findProduct.DefaultPrice = this.GetDisplayPrice(market);
            findProduct.DefaultPriceAmount = this.GetDefaultPriceAmount(market);

            PriceAndMarket discountPrice = this.GetDiscountPrice(market);
            findProduct.DiscountedPriceAmount = GetPriceWithCheck(discountPrice);
            findProduct.DiscountedPrice = GetDisplayPriceWithCheck(discountPrice);

            PriceAndMarket customerClubPrice = this.GetCustomerClubPrice(market);
            findProduct.CustomerClubPriceAmount = GetPriceWithCheck(customerClubPrice);
            findProduct.CustomerClubPrice = GetDisplayPriceWithCheck(customerClubPrice);

            findProduct.Brand = this.Facet_Brand;
            return findProduct;
        }

        private double GetPriceWithCheck(PriceAndMarket price)
        {
            return price != null ? (double)price.UnitPrice.Amount : 0;
        }

        private string GetDisplayPriceWithCheck(PriceAndMarket price)
        {
            return price != null ? price.UnitPrice.ToString() : string.Empty;
        }


        public bool ShouldIndex()
        {
            // Is this part of a product, or stand alone?
            var products = GetProductByVariant(this.ContentLink);
            if(products != null && products.Any())
            {
                return false;
            }

            return !(StopPublish != null && StopPublish < DateTime.Now);
        }

        public IEnumerable<ProductVariation> GetProductByVariant(ContentReference variation)
        {
            var linksRepository = ServiceLocator.Current.GetInstance<ILinksRepository>();
            var allRelations = linksRepository.GetRelationsByTarget(variation);

            // Relations to Product is ProductVariation
            return allRelations.OfType<ProductVariation>().ToList();
        }

        public ProductListViewModel Populate(Mediachase.Commerce.IMarket currentMarket)
        {
            ProductListViewModel productListViewModel = new ProductListViewModel(this, currentMarket, CustomerContext.Current.CurrentContact);
            
            return productListViewModel;
        }

        [ScaffoldColumn(false)]
        public virtual string ContentAssetIdInternal { get; set; }
        public Guid ContentAssetsID
        {
            get
            {
                Guid assetId;
                if (Guid.TryParse(ContentAssetIdInternal, out assetId))
                    return assetId;
                return Guid.Empty;
            }
            set
            {
                ContentAssetIdInternal = value.ToString();
                this.ThrowIfReadOnly();
                IsModified = true;
            }
        }
    }
}