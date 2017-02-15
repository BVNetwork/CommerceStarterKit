using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using OxxCommerceStarterKit.Core.Attributes;
using EPiServer;

namespace OxxCommerceStarterKit.Web.Models.PageTypes
{
    [ContentType(DisplayName = "Landing Page", 
        GUID = "3b835f52-0b78-474e-8968-b41f15085847", 
        Description = "Landing page campaign page type", 
        GroupName = WebGlobal.GroupNames.Campaign)]
    [SiteImageUrl(thumbnail: EditorThumbnail.Multimedia)]
    public class LandingPage : SitePage
    {
        [Display(
            Name = "Content area narrow top",
            GroupName = SystemTabNames.Content,
            Order = 100)]
        [CultureSpecific]
        public virtual ContentArea ContentAreaNarrowTop { get; set; }

        [Display(
            Name = "Content area wide top",
            GroupName = SystemTabNames.Content,
            Order = 110)]
        [CultureSpecific]
        public virtual ContentArea ContentAreaWideTop { get; set; }

        [Display(
            Name = "Content area first small",
            GroupName = SystemTabNames.Content,
            Order = 120)]
        [CultureSpecific]
        public virtual ContentArea ContentAreaFirstSmall { get; set; }

        [Display(
            Name = "Content area second small",
            GroupName = SystemTabNames.Content,
            Order = 130)]
        [CultureSpecific]
        public virtual ContentArea ContentAreaSecondSmall { get; set; }
        [Display(
            Name = "Content area third small",
            GroupName = SystemTabNames.Content,
            Order = 140)]
        [CultureSpecific]
        public virtual ContentArea ContentAreaThirdSmall { get; set; }

        [Display(
            Name = "Content area narrow bottom",
            GroupName = SystemTabNames.Content,
            Order = 150)]
        [CultureSpecific]
        public virtual ContentArea ContentAreaNarrowBottom { get; set; }

        [Display(
            Name = "Content area narrow bottom",
            GroupName = SystemTabNames.Content,
            Order = 160)]
        [CultureSpecific]
        public virtual ContentArea ContentAreaWideBottom { get; set; }

        [Display(
            Name = "List view image",
            GroupName = "Metadata",
            Order = 1000)]
        [UIHint(WebGlobal.SiteUIHints.MediaUrl)]
        public virtual Url ListViewImage { get; set; }

    }
}