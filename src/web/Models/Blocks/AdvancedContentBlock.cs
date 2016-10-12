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
using EPiServer.Web;
using Newtonsoft.Json;
using OxxCommerceStarterKit.Core.Attributes;
using OxxCommerceStarterKit.Web.Business.Rendering;
using OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories;

namespace OxxCommerceStarterKit.Web.Models.Blocks
{
	[ContentType(
        DisplayName = "Advanced Content",
        Description = "Free form content block allowing HTML, CSS and Javascript. Use with care.",
        GUID = "0EF40455-51E5-4BEA-8D89-5D77786CCA8C",
        GroupName= WebGlobal.GroupNames.Specialized)]
	[SiteImageUrl(thumbnail: EditorThumbnail.System)]
	public class AdvancedContentBlock : SiteBlockData, IDefaultDisplayOption
	{
		[Display(
			GroupName = SystemTabNames.Content,
			Order = 10)]
		[CultureSpecific]
        [UIHint(UIHint.Textarea)]
		public virtual string HtmlContent{ get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 30)]
        [CultureSpecific]
        [UIHint(UIHint.Textarea)]
        public virtual string Css { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 60)]
        [CultureSpecific]
        [UIHint(UIHint.Textarea)]
        public virtual string Javascript { get; set; }

        public string Tag
	    {
	        get { return WebGlobal.ContentAreaTags.FullWidth; }
	    }

	}
}
