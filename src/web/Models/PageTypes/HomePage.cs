/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using OxxCommerceStarterKit.Core.Attributes;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.Blocks.Base;

namespace OxxCommerceStarterKit.Web.Models.PageTypes
{
    [ContentType(GUID = "3B579852-D4AD-41D5-B4BA-50FF4CC55A6A",
        DisplayName = "Home Page",
        Description = "The start page of the site",
        GroupName = WebGlobal.GroupNames.Default,
		AvailableInEditMode = false,
		Order = 100)]
    [SiteImageUrl]
    public class HomePage : SitePage
    {

        [Searchable(false)]
        [Display(
            Name = "Body Content",
            Description = "BodyContent",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        [CultureSpecific]
        public virtual ContentArea BodyContent { get; set; }

        [Searchable(false)]
        [Display(
            Name = "Wide content area",
            Description = "Wide content area at front page top",
            GroupName = SystemTabNames.Content,
            Order = 5)]
        [CultureSpecific]
        public virtual ContentArea ContentAreaWide { get; set; }
      
        [Searchable(false)]
        [Display(
            Name = "TopLeftMenu",
            Description = "The menu at top left",
            GroupName = SystemTabNames.Settings,
            Order = 10)]
        [CultureSpecific]
        public virtual LinkItemCollection TopLeftMenu { get; set; }

        [Searchable(false)]
        [Display(
            Name = "TopRightMenu",
            Description = "The menu at top right",
            GroupName = SystemTabNames.Settings,
            Order = 20)]
        [CultureSpecific]
        public virtual LinkItemCollection TopRightMenu { get; set; }

        [Searchable(false)]
        [Display(Name = "The Logo to use on the site",
            GroupName = SystemTabNames.Settings,
            Order = 30)]
        [CultureSpecific]
        [UIHint(UIHint.Image)]
        public virtual ContentReference LogoImage { get; set; }

        [Searchable(false)]
        [Display(
            Name = "Global Footer Content",
            Description = "Content shown in the footer of all pages",
            GroupName = SystemTabNames.Settings,
            Order = 40)]
        [CultureSpecific]
        public virtual ContentArea GlobalFooterContent { get; set; }

        [Searchable(false)]
        [Display(
            Name = "Footer menu root folder",
            Description = "The folder who's children will be in the footer menu",
            GroupName = SystemTabNames.Settings,
            Order = 50)]
        public virtual PageReference FooterMenuFolder { get; set; }

        [Searchable(false)]
        [Display(
            Name = "Social Media",
            Description = "Social Media Links",
            GroupName = SystemTabNames.Settings,
            Order = 60)]
        [AllowedTypes(new[] { typeof(SocialMediaLinkBlock) })]
        [CultureSpecific]
        [UIHint("FloatingEditor")]
        public virtual ContentArea SocialMediaIcons { get; set; }


        [Display(
            Name = "Site Settings",
            Description = "Global settings for this site.",
            GroupName = SystemTabNames.Settings,
            Order = 100)]
        public virtual SettingsBlock Settings { get; set; }


        [CultureSpecific]
        [Display(
            Name = "Recommendations header",
            Description = "Section header",
            GroupName = SystemTabNames.Content,
            Order = 500)]
        public virtual string RecommendationsHeader { get; set; }


        [Display(
            Name = "Recommendations section",
            Description = "Section header",
            GroupName = SystemTabNames.Content,
            Order = 510)]
        public virtual        SectionBlockBase RecommendationSection { get; set; }
    }
}
