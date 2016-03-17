using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Marketing.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace OxxCommerceStarterKit.Web.Promotion
{
    [ContentType(DisplayName = "Buy N products, get free product",
        Description = "Buy a specified number of products from one or more categories, get a free product.",
        GUID="FACC7148-9AE2-48A3-A86A-3E9E1C1505C5")]
    public class BuyXFromCategoryGetProductForFree : OrderPromotion
    {
        [UIHint("allcontent"), AllowedTypes(new Type[] { typeof(NodeContent) })]
        [PromotionRegion(PromotionRegionName.Condition)]
        [Display(Order = 10)]
        public virtual ContentReference Category { get; set; }

        [PromotionRegion(PromotionRegionName.Condition)]
        [Display(Description = "The number of products needed",
            Order = 20)]
        public virtual int Threshold { get; set; }


        [UIHint("allcontent"), AllowedTypes(new Type[] { typeof(VariationContent) })]
        [PromotionRegion(PromotionRegionName.Reward)]
        [Display(Order = 30)]
        public virtual ContentReference FreeProduct { get; set; }


       
    }    
}
