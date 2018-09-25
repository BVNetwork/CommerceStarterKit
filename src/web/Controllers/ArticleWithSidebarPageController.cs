/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Framework.DataAnnotations;
using EPiServer.GoogleAnalytics.Helpers;
using EPiServer.Security;
using EPiServer.Tracking.PageView;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Web.Business.Analytics;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    [TemplateDescriptor()]
    public class ArticleWithSidebarPageController : PageControllerBase<ArticleWithSidebarPage>
    {
		private readonly IContentLoader _contentLoader;

        public ArticleWithSidebarPageController(IContentLoader contentLoader)
        {            
			_contentLoader = contentLoader;		    
        }

        [PageViewTracking]
        public ViewResult Index(PageData currentPage)
        {
            var viewPath = GetViewForPageType(currentPage);

            var model = CreatePageViewModel(currentPage);

            return View(viewPath, model);
        }
    }
}
