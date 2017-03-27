using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.ModelBuilders
{
    [ServiceConfiguration(typeof(IQuickBuyModelBuilder))]
    public class QuickBuyViewModelBuilder : IQuickBuyModelBuilder
    {
        private readonly IContentLoader _contentLoader;

        public QuickBuyViewModelBuilder(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        public QuickBuyViewModel Build(QuickBuyBlock currentBlock, QuickBuyViewModel model)
        {
            var productInfo =
                currentBlock != null ? 
                (currentBlock.CampaignProducts != null ? 
                (currentBlock.CampaignProducts.Items != null ? 
                currentBlock.CampaignProducts.Items.Select(x => _contentLoader.Get<VariationContent>(x.ContentLink)) : null) : null) : null;
            
            model.Products = productInfo != null ? 
                productInfo.Select(x => new ProductInfo() {Sku = x.Code, Name = x.DisplayName}) : new List<ProductInfo>();



            return model;
        }
    }

    public interface IQuickBuyModelBuilder
    {
        QuickBuyViewModel Build(QuickBuyBlock currentBlock, QuickBuyViewModel model);
    }
}