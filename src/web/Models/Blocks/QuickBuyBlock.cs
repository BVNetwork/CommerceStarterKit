using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace OxxCommerceStarterKit.Web.Models.Blocks
{
    [ContentType(DisplayName = "Quick Buy Block", GUID = "6f4844ab-6a2e-4384-a98f-2089f1571d87", Description = "")]
    public class QuickBuyBlock : BlockData
    {
        [Display(Name = "Promotion Id", GroupName = WebGlobal.GroupNames.Analytics)]
        public virtual string PromotionId { get; set; }

        [Display(Name = "Promotion Name", GroupName = WebGlobal.GroupNames.Analytics)]
        public virtual string PromotionName { get; set; }

        [Display(Name = "Promotion Banne Name", GroupName = WebGlobal.GroupNames.Analytics)]
        public virtual string PromotionBannerName { get; set; }

        [Display(Name = "Campaign Products")]
        [AllowedTypes(new[] {typeof(VariationContent), typeof(ProductContent)})]
        public virtual ContentArea CampaignProducts { get; set; }

        public virtual ContentReference Image { get; set; }

        public virtual XhtmlString Content { get; set; }

        public virtual XhtmlString Disclaimer { get; set; }

        public virtual bool RequireDisclaimer { get; set; }

        public virtual string CouponCode { get; set; }

        [Display(Name="Submit label")]
        public virtual string ButtonLabel { get; set; }
        
        [Display(Name="Redirect to page on success")]
        public virtual ContentReference RedirectPage { get; set; }
    }
}