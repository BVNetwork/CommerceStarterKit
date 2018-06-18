/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Business.Rss;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Extensions;

namespace OxxCommerceStarterKit.Web.Controllers
{
	public class PageControllerBase<T> : PageController<T> where T : PageData
	{
		private static Injected<IContentLoader> _contentLoaderService ;
        protected static ILogger _log = LogManager.GetLogger();

		protected IContentLoader ContentLoader
		{
			get { return _contentLoaderService.Service; }
		}
 
		protected T CurrentPage
		{
			get
			{
				return PageContext.Page as T;
			}
		}

		protected override void OnAuthorization(AuthorizationContext filterContext)
		{
			CheckAccess(filterContext);
			base.OnAuthorization(filterContext);
		}

		private void CheckAccess(AuthorizationContext filterContext)
		{

		    var contentLink = filterContext.RequestContext.GetContentLink();

		    if (contentLink == null)
		        return;

		    var content = ContentLoader.Get<IContent>(contentLink, new LoaderOptions {LanguageLoaderOption.Fallback()});
		    
		    if (content != null && content.QueryAccess().HasFlag(AccessLevel.Read))
		    {
		        return;
		    }

			ServeAccessDenied(filterContext);
		}



		private void ServeAccessDenied(AuthorizationContext filterContext)
		{
			_log.Information(
				"AccessDenied",
				new AccessDeniedException(CurrentPage.ContentLink));

			AccessDeniedDelegate accessDenied
				= AccessDeniedHandler.CreateAccessDeniedDelegate(filterContext);
			accessDenied(filterContext);
		}

        protected virtual string GetViewForPageType(PageData currentPage)
        {
            var virtualPath = String.Format("~/Views/{0}/Index.cshtml", currentPage.GetOriginalType().Name);
            if (System.IO.File.Exists(Request.MapPath(virtualPath)) == false)
            {
                virtualPath = "Index";
            }
            return virtualPath;
        }


		public virtual IPageViewModel<PageData> CreatePageViewModel(PageData pageData)
		{
			var activator = new Activator<IPageViewModel<PageData>>();
			var model = activator.Activate(typeof(PageViewModel<>), pageData);
			InitializePageViewModel(model);
			return model;
		}

	    protected void InitializePageViewModel<TViewModel>(TViewModel model) where TViewModel : IPageViewModel<PageData>
		{
			var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
			if (ContentReference.IsNullOrEmpty(ContentReference.StartPage) == false)
			{
                // TODO: Use the Chrome instead
				HomePage startPage = contentLoader.Get<HomePage>(ContentReference.StartPage);

				model.TopLeftMenu = model.TopLeftMenu ?? startPage.TopLeftMenu;
				model.TopRightMenu = model.TopRightMenu ?? startPage.TopRightMenu;
				model.SocialMediaIcons = model.SocialMediaIcons ?? startPage.SocialMediaIcons;
				if (model.CurrentPage != null)
				{
					model.Section = model.Section ?? GetSection(model.CurrentPage.ContentLink);
				}
				else
				{
					model.Section = model.Section ?? GetSection(startPage.ContentLink);
				}
				model.LoginPage = model.LoginPage ?? startPage.Settings.LoginPage;
				model.AccountPage = model.AccountPage ?? startPage.Settings.AccountPage;
			    model.Language = string.IsNullOrEmpty(model.Language) == false ? model.Language : startPage.LanguageBranch;
				model.CheckoutPage = model.CheckoutPage ?? startPage.Settings.CheckoutPage;
			}
		}



        /// <summary>
        /// Returns the closest parent to the start page of the given page.
        /// </summary>
        /// <remarks>
        /// Start Page
        ///     - About Us (This is the section)
        ///         - News
        ///             News 1 (= contentLink parameter)
        /// </remarks>
        /// <param name="contentLink">The content you want to find the section for</param>
        /// <returns>The parent page closes to the start page, or the page referenced by the contentLink itself</returns>
        protected IContent GetSection(ContentReference contentLink)
		{
			var currentContent = ContentLoader.Get<IContent>(contentLink);
			if (currentContent.ParentLink != null && currentContent.ParentLink.CompareToIgnoreWorkID(ContentReference.StartPage))
			{
				return currentContent;
			}

            // Loop upwards until the parent is start page or root
			return ContentLoader.GetAncestors(contentLink)
				.OfType<PageData>()
				.SkipWhile(x => x.ParentLink == null || !x.ParentLink.CompareToIgnoreWorkID(ContentReference.StartPage))
				.FirstOrDefault();
		}

	    public ActionResult Rss(PageData currentPage)
	    {
	        var urlHelper = ServiceLocator.Current.GetInstance<UrlHelper>();

            if (Request.Url != null)
	        {
	            string pageBaseUrl = string.Format("{0}://{1}{2}", "https", Request.Url.Host,
	                Request.Url.IsDefaultPort ? string.Empty : ":" + Request.Url.Port);

                var items = new List<SyndicationItem>();

	            foreach (var childPage in _contentLoaderService.Service.GetChildren<PageData>(currentPage.ContentLink))
	            {
	                var itemIntro = childPage["Intro"] != null ? childPage["intro"].ToString().StripHtml() : string.Empty;
	                var url = new Uri(pageBaseUrl + Url.ContentUrl(childPage.ContentLink));
                    var item = new SyndicationItem(childPage.Name, itemIntro, url);
	                item.LastUpdatedTime = childPage.Changed;

                    var itemImageUrl = childPage["ListViewImage"] != null ? pageBaseUrl + urlHelper.ContentUrl((Url)childPage["ListViewImage"]) + "?preset=listmedium" : string.Empty;
                    if(!string.IsNullOrWhiteSpace(itemImageUrl))
                        item.SetEnclosure(itemImageUrl);

                    items.Add(item);
	            }

	            var intro = currentPage["Intro"] != null ? currentPage["Intro"].ToString().StripHtml() : string.Empty;
	            var imageUrl = currentPage["ListViewImage"] != null ? pageBaseUrl + urlHelper.ContentUrl((Url)currentPage["ListViewImage"]) + "?preset=listmedium" : string.Empty;
                var feed = new SyndicationFeed(currentPage.Name, intro, new Uri(Request.Url.AbsoluteUri), items);
	            feed.LastUpdatedTime = currentPage.Changed;
                if(!string.IsNullOrWhiteSpace(imageUrl))
                    feed.ImageUrl = new Uri(imageUrl);

                return new FeedResult(new Rss20FeedFormatter(feed));
	        }
	        return null;
	    }	    
    }
}
