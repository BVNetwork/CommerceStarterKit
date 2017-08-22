using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Models.Catalog;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class GenericProductViewModel : CatalogContentViewModel<GenericProductContent>
    {
        public GenericProductViewModel(GenericProductContent currentContent) : base(currentContent)
        {
            CatalogContent = currentContent;
        }

        public IEnumerable<SelectListItem> Size { get; set; }
        public IVariationViewModel<GenericSizeVariationContent> GenericVariationViewModel { get; set; }

        private List<MediaData> _media;
        public List<MediaData> Media
        {
            get
            {
                if (_media == null)
                {
                    var contentLoader = ServiceLocator.Current.GetInstance<EPiServer.IContentLoader>();
                    List<ContentReference> mediaReferences = ContentWithAssets.AssetImageUrls();
                    _media = new List<MediaData>();
                    foreach (var mediaReference in mediaReferences)
                    {
                        var file = contentLoader.Get<MediaData>(mediaReference);
                        if (file != null)
                        {
                            _media.Add(file);
                        }
                    }
                }
                return _media;
            }
        }

        public bool IsSellable { get; set; }

        public List<ProductListViewModel> ProductAlternatives { get; set; }
        public List<ProductListViewModel> ProductCrossSell { get; set; }
    }
}