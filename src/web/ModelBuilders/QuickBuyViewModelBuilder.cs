using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.Files;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.ModelBuilders
{
    [ServiceConfiguration(typeof(IQuickBuyModelBuilder))]
    public class QuickBuyViewModelBuilder : IQuickBuyModelBuilder
    {
        private readonly IContentLoader _contentLoader;
        private readonly LocalizationService _localizationService;

        public QuickBuyViewModelBuilder(IContentLoader contentLoader, LocalizationService localizationService)
        {
            _contentLoader = contentLoader;
            _localizationService = localizationService;
        }

        public QuickBuyViewModel Build(QuickBuyBlock currentBlock, QuickBuyViewModel model)
        {
            model.CurrentBlock = currentBlock;            
            var productInfo =
                currentBlock != null ? 
                (currentBlock.CampaignProducts != null ? 
                (currentBlock.CampaignProducts.Items != null ? 
                currentBlock.CampaignProducts.Items.Select(x => _contentLoader.Get<VariationContent>(x.ContentLink)) : null) : null) : null;
            
            model.Products = productInfo != null ? 
                productInfo.Select(x => new ProductInfo() {Sku = x.Code, Name = x.DisplayName}) : new List<ProductInfo>();

            if (currentBlock != null && currentBlock.Image != null)
                model.ImageContent = new ImageViewModel(_contentLoader.Get<ImageFile>(currentBlock.Image),"en");

            if (currentBlock != null)
            {
                model.ActionLabel = currentBlock.ButtonLabel ?? string.Empty;
            }

            if (string.IsNullOrEmpty(model.ActionLabel))
            {
                model.ActionLabel = _localizationService.GetString("/common/quickbuy/form/buy");
            }

            return model;
        }
    }

    public interface IQuickBuyModelBuilder
    {
        QuickBuyViewModel Build(QuickBuyBlock currentBlock, QuickBuyViewModel model);
    }
}