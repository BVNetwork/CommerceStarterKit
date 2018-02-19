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
using OxxCommerceStarterKit.Core.Attributes;

namespace OxxCommerceStarterKit.Web.Models.PageTypes
{
	[ContentType(GUID = "5dbe7b83-38e0-43f6-9663-0611829f7949",
	 DisplayName = "Register user page",
	 Description = "Settings for the register user page and welcome email",
     GroupName = WebGlobal.GroupNames.Specialized,
	 AvailableInEditMode = false, 
	 Order = 100)]
    [SiteImageUrl(thumbnail: EditorThumbnail.Social)]
	public class RegisterPage : SitePage
	{
		[Display(
			Name = "Content area top",
			GroupName = SystemTabNames.Content,
			Order = 10)]
		public virtual ContentArea PreBodyContent { get; set; }

		[Display(
			Name = "Welcome email subject",
			GroupName = SystemTabNames.Content,
			Order = 20)]
		[CultureSpecific]
		public virtual string EmailSubject { get; set; }

		[Display(
			Name = "Welcome email text",
			GroupName = SystemTabNames.Content,
			Order = 40)]
		[CultureSpecific]
		public virtual XhtmlString EmailBody { get; set; }

	    [Display(
	        Name = "Post register page",
	        GroupName = SystemTabNames.Content,
	        Order = 50)]
	    [CultureSpecific]
	    public virtual ContentReference PostRegisterPage { get; set; }
    }
}
