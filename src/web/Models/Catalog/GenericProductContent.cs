using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer;
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
using OxxCommerceStarterKit.Web.Extensions;
using OxxCommerceStarterKit.Web.Models.Blocks.Contracts;
using OxxCommerceStarterKit.Web.Models.Catalog.Base;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Models.Catalog
{
     [CatalogContentType(GUID = "1F13773D-14B0-4F32-9E04-20BA9B2B29F1", MetaClassName = "GenericProductContent",
       DisplayName = "Generic Product",
       Description = "A generic product with generic fields",
       GroupName = WebGlobal.GroupNames.Commerce
       )]
    public class GenericProductContent : ProductBase, IIndexableContent, IProductListViewModelInitializer, IResourceable
    {

         // Same for all languages
         [Display(Name = "Facet Brand",
             Order = 12)]
         public virtual string Facet_Brand { get; set; }

         // Multi lang
         [Display(Name = "Color", Order = 15)]
         [CultureSpecific]
         public virtual string Color { get; set; }

        // Multi lang
        [Display(Name = "Overview", Order = 20)]
        [CultureSpecific]
        public virtual XhtmlString Overview { get; set; }

        // Multi lang
        [Display(Name = "Details", Order = 30)]
        [CultureSpecific]
        public virtual XhtmlString Details { get; set; }

        [Display(
         GroupName = SystemTabNames.Content,
         Order = 300,
         Name = "Average Rating")]
        [Editable(false)]
        public virtual double AverageRating { get; set; }


        public FindProduct GetFindProduct(IMarket market)
        {
            List<VariationContent> productVariants = this.GetVaritions().ToList();
            var variations = GetGenericVariants(productVariants, market);

            var language = (Language == null ? string.Empty : Language.Name);
         
            var findProduct = new FindProduct(this, language);

            findProduct.Description = Description;
            findProduct.Color = Color != null ? new List<string>() {Color} : new List<string>();
            findProduct.Sizes =
                variations.Select(x => x.Size ?? string.Empty).Distinct().ToList();
            findProduct.Brand = this.Facet_Brand;

            findProduct.SetPriceData(productVariants, market);

            findProduct.GenericVariants = variations;

            return findProduct;
        }

        private List<GenericFindVariant> GetGenericVariants(IEnumerable<VariationContent> productVariants, IMarket market)
        {
            if(productVariants == null)
                return null;

            List<GenericFindVariant> variations = new List<GenericFindVariant>();
            foreach (var variation in productVariants)
            {
                if (variation is GenericSizeVariationContent)
                {

                    GenericSizeVariationContent genericSizeVariationContent = variation as GenericSizeVariationContent;
                    var genericVariation = new GenericFindVariant()
                    {
                        Id = genericSizeVariationContent.ContentLink.ID,
                        Color = new List<string>{genericSizeVariationContent.Color},
                        Size = genericSizeVariationContent.Size ?? string.Empty,
                        Prices = genericSizeVariationContent.GetPricesWithMarket(market),
                        Code = genericSizeVariationContent.Code,
                        Stock = genericSizeVariationContent.GetStock()
                    };
                    variations.Add(genericVariation);
                }

            }
            return variations;
        }

         public bool ShouldIndex()
         {
             return !(StopPublish != null && StopPublish < DateTime.Now);
         }

         
         public ProductListViewModel Populate(IMarket market)
         {
             ProductListViewModel productListViewModel = new ProductListViewModel(this, market, CustomerContext.Current.CurrentContact)
             {
                 BrandName = Facet_Brand,
             };

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