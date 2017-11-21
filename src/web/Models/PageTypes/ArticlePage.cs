/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using OxxCommerceStarterKit.Core.Attributes;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Models.ViewModels.Contracts;

namespace OxxCommerceStarterKit.Web.Models.PageTypes
{
    [ContentType(GUID = "d7cdf1da-83a0-4f9e-b602-55017ad333ee",
	 DisplayName = "Article",
	 Description = "An article template",
     GroupName = WebGlobal.GroupNames.Default,
	 Order = 100)]
    [SiteImageUrl(thumbnail: EditorThumbnail.Content)]
	public class ArticlePage : SitePage, IHasFeatureProduct, IHasListViewContentItem
    {
		[Display(
			Name = "List view image",
			GroupName = SystemTabNames.Content,
			Order = 11)]
		[UIHint(WebGlobal.SiteUIHints.MediaUrl)]
		public virtual Url ListViewImage { get; set; }

		[Display(
			Name = "List view text",
			Description = "Used in lists, if empty, then the intro will be used instead",
			GroupName = SystemTabNames.Content,
			Order = 10)]
        [CultureSpecific]
		public virtual string ListViewText { get; set; }

		[Display(
			Name = "Content area top",
			GroupName = SystemTabNames.Content,
			Order = 15)]
        [CultureSpecific]
		public virtual ContentArea PreBodyContent { get; set; }

		[Display(
			Name = "Page title",
			GroupName= SystemTabNames.Content,
			Order = 20)]
        [CultureSpecific]
		public virtual string PageTitle { get; set; }

		[Display(
			Name = "Intro",
			GroupName = SystemTabNames.Content,
			Order = 30)]
        [CultureSpecific]
		public virtual XhtmlString Intro { get; set; }

		[Display(
			Name = "Body",
			GroupName = SystemTabNames.Content,
			Order = 40)]
		[CultureSpecific]
		public virtual XhtmlString BodyText { get; set; }

		[Display(
		Name = "Content area bottom",
		GroupName = SystemTabNames.Content,
		Order = 50)]
        [CultureSpecific]
		public virtual ContentArea BodyContent { get; set; }

        [Display(Name = "Menu feature product", 
            Description = "", GroupName = SystemTabNames.Content, Order = 60)]
        public virtual FeatureProductBlock FeatureProduct { get; set; }


        public IListViewContentItem GetListViewContentItem()
        {
            var item = new ListViewContentItem();
            item.Title = PageTitle ?? PageName;
            item.Intro = ListViewText;
            if(Intro != null)
            {
                item.Intro =  this.Intro.ToString();
            }
            if(this.ListViewImage != null)
            {
                item.ImageUrl = this.ListViewImage.ToString();
            }

            item.ContentLink = this.ContentLink;

            return item;
        }
    }
}
