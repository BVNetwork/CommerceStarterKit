using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Models.Blocks.Contracts;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Marketing;
using EPiServer.Shell.ObjectEditing;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Core.Models;
using OxxCommerceStarterKit.Web.Business.Rendering;
using OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories;
using OxxCommerceStarterKit.Web.Extensions;

namespace OxxCommerceStarterKit.Web.Models.Catalog
{
    [CatalogContentType(GUID = "252BFD8F-EF0C-49FB-86D8-31E2436D1461", MetaClassName = "DigitalCameraSKU",
       DisplayName = "Camera",
       Description = "Digital Camera",
       GroupName = "Camera"
       )]
    public class DigitalCameraVariationContent : VariationContent, IFacetBrand, IIndexableContent, IProductListViewModelInitializer, IResourceable
    {

        [Display(Name = "BuyButtonColor",
            GroupName = SystemTabNames.Content,
            Order = 2)]
        [SelectOne(SelectionFactoryType = typeof(ButtonColorSelectionFactory))]
        public virtual string BuyButtonColor { get; set; }

        // Multi lang
        [Display(Name = "Color", Order = 5)]
        [CultureSpecific]
        public virtual string Color { get; set; }
        
        // Multi lang
        [Display(Name = "Description", Order = 10)]
        [CultureSpecific]
        public virtual XhtmlString Description { get; set; }

        // Multi lang
        [Display(Name = "Overview", Order = 20)]
        [CultureSpecific]
        public virtual XhtmlString Overview { get; set; }

        [Display(Name = "Resolution",
           Description = "Number in megapixels",
           Order = 30)]
        public virtual decimal Resolution { get; set; }

        [Display(Name = "Lens Mount",
           Description = "",
           Order = 40)]
        public virtual string LensMount { get; set; }


        [Display(Name = "Camera Format",
           Description = "Full-frame, 1.3 crop etc.",
           Order = 50)]
        public virtual string CameraFormat { get; set; }

        [Display(Name = "File Format",
           Description = "Information like image formats, video formats etc.",
           Order = 60)]
        public virtual string FileFormat { get; set; }

        [Display(Name = "Connectivity",
           Description = "Wifi, bluetooth ect.",
           Order = 70)]
        public virtual string Connectivity { get; set; }

        [Display(Name = "Memory Card Type",
           Description = "",
           Order = 70)]
        public virtual string MemoryCardType { get; set; }

        [Display(Name = "Battery",
           Description = "",
           Order = 80)]
        public virtual string Battery { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 90,
            Name = "Focus Control")]
        [CultureSpecific(true)]
        public virtual XhtmlString FocusControl { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 300,
            Name = "Average Rating")]
        [Editable(false)]
        public virtual double AverageRating { get; set; }

        // Same for all languages
        [Display(Name = "Brand",
            Order = 18)]
        public virtual string Facet_Brand { get; set; }

        public FindProduct GetFindProduct(Mediachase.Commerce.IMarket market)
        {
            var language = (Language == null ? string.Empty : Language.Name);
            var findProduct = new FindProduct(this, language);

            findProduct.Description = Description;
            findProduct.Overview = Overview;

            findProduct.SetPriceData(this, market);

            findProduct.Brand = Facet_Brand;
            findProduct.Resolution = Resolution;
            findProduct.LensMount = LensMount;
            findProduct.CameraFormat = CameraFormat;
            findProduct.FileFormat = FileFormat;
            findProduct.Connectivity = Connectivity;
            findProduct.Battery = Battery;
            findProduct.MemoryCardType = MemoryCardType;
            findProduct.Weight = Weight;


            return findProduct;
        }


        public bool ShouldIndex()
        {
            return !(StopPublish != null && StopPublish < DateTime.Now);
        }

        public ProductListViewModel Populate(Mediachase.Commerce.IMarket currentMarket)
        {
            ProductListViewModel productListViewModel = new ProductListViewModel(this, currentMarket, CustomerContext.Current.CurrentContact)
            {
                BrandName = Facet_Brand
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