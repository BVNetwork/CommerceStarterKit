/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Web.Mvc;
using EPiServer.Tracking.PageView;
using EPiServer.Web.Mvc;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class SearchPageController : PageController<SearchPage>
    {
        [PageViewTracking]
        public ActionResult Index(SearchPage currentPage, string q)
        {
            var model = new SearchPageViewModel(currentPage);
            model.NumberOfProductsToShow = currentPage.NumberOfProductsToShow > 0 ? currentPage.NumberOfProductsToShow : 9;
            model.NumberOfArticlesToShow = currentPage.NumberOfArticlesToShow > 0
                ? currentPage.NumberOfArticlesToShow
                : 5;
            model.Language = currentPage.Language.Name;
            model.SearchedQuery = q;
            return View(model);
        }
    }
}
