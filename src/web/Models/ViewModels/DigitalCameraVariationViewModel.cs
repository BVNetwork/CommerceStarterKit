using System.Collections.Generic;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Models.Catalog;
using Price = EPiServer.Commerce.SpecializedProperties.Price;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class DigitalCameraVariationViewModel : CatalogContentViewModel<VariationContent>
    {
        public DigitalCameraVariationContent CatalogVariationContent { get; set; }
        public List<MediaData> Media { get; set; }
        public Price Price { get; set; }
        public CartItemModel CartItem { get; set; }

        public DigitalCameraVariationViewModel(DigitalCameraVariationContent currentContent)
            : base(currentContent)
        {
            CatalogVariationContent = currentContent;
            Media = GetMedia(currentContent);
            CartItem = new CartItemModel(CatalogVariationContent){CanBuyEntry = true};
        }
        private List<MediaData> GetMedia(DigitalCameraVariationContent currentContent)
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
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
        public List<ProductListViewModel> ProductAlternatives { get; set; }
        public List<ProductListViewModel> ProductCrossSell { get; set; }
    }


}