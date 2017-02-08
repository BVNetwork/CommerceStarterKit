/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.WebPages;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Editor;
using EPiServer.Find.Cms;
using EPiServer.ServiceLocation;
using EPiServer.UI.Report;
using EPiServer.Web;
using EPiServer.Web.Routing;
using OxxCommerceStarterKit.Web.Helpers;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Models.ViewModels.Contracts;

namespace OxxCommerceStarterKit.Web.Business
{
    public class PageContextActionFilter : IResultFilter
    {
        private readonly IContentLoader _contentLoader;
        private readonly UrlResolver _urlResolver;
        private readonly ILanguageBranchRepository _languageBranchRepository;
        private readonly ISiteSettingsProvider _siteConfiguration;
        // private readonly ViewModelFactory _modelFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageContextActionFilter"/> class.
        /// </summary>
        public PageContextActionFilter(IContentLoader contentLoader, UrlResolver urlResolver, ILanguageBranchRepository languageBranchRepository, ISiteSettingsProvider siteConfiguration)
        {
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
            _languageBranchRepository = languageBranchRepository;
            _siteConfiguration = siteConfiguration;
        }

        /// <summary>
        /// Called before an action result executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            HomePage homePage = _siteConfiguration.GetStartPage();
            if (homePage != null)
            {
                SettingsBlock settings = homePage.Settings;

                // This can actually be null if we have a problem with our language settings
                if (settings != null)
                {
                    var chrome = PopulateChrome(homePage, settings);

                    filterContext.Controller.ViewBag.Chrome = chrome;
                }
            }

            var viewModel = filterContext.Controller.ViewData.Model;

            var model = viewModel as IPageViewModel<SitePage>;
            if (model != null)
            {
                PopulateOpenGraph(filterContext, model);
                var chrome = filterContext.Controller.ViewBag.Chrome;
                filterContext.Controller.ViewBag.Chrome = PopulateChromePageSettings(chrome, model);
            }
        }

        protected virtual Chrome PopulateChromePageSettings(Chrome chrome, IPageViewModel<SitePage> model)
        {
            if(chrome != null && model != null && model.CurrentPage != null) {
                SitePage sitePage = model.CurrentPage as SitePage;
                if(sitePage != null) {
                    chrome.HideHeader = sitePage.HideSiteHeader;
                    chrome.HideFooter = sitePage.HideSiteFooter;
                }
            }
            return chrome;
        }

        protected virtual void PopulateOpenGraph(ResultExecutingContext filterContext, IPageViewModel<SitePage> model)
        {
            var siteUrl = SiteDefinition.Current.SiteUrl.ToString();
            siteUrl = siteUrl.TrimEnd('/');

            // When in preview mode, this is also run for some of the block, they have no CurrentPage
            if (model.CurrentPage == null)
                return;

            OpenGraphModel openGraph = new OpenGraphModel();
            openGraph.Url = siteUrl + _urlResolver.GetUrl(model.CurrentPage.ContentLink);
            openGraph.ContentType = "article";

            // The list view model works best
            var listItem = model.CurrentPage as IHasListViewContentItem;
            if (listItem != null)
            {
                var listItemModel = listItem.GetListViewContentItem();
                openGraph.Description = listItemModel.Intro;
                openGraph.Title = listItemModel.Title;
                if (listItemModel.ImageUrl != null)
                {
                    openGraph.ImageUrl = siteUrl +
                                         _urlResolver.GetUrl(new UrlBuilder(listItemModel.ImageUrl), ContextMode.Default)
                                         + "?preset=ogpage";
                }
            }
            else
            {
                openGraph.Title = model.CurrentPage.MetaTitle;
                openGraph.Description = model.CurrentPage.MetaDescription;
                // See if there is an image we can use
                var imageUrlProp = model.CurrentPage.GetPropertyValue<Url>("ListViewImage");
                if (imageUrlProp != null)
                {
                    var imageUrl = _urlResolver.GetUrl(new UrlBuilder(imageUrlProp), ContextMode.Default);
                    openGraph.ImageUrl = siteUrl + imageUrl + "?preset=ogpage";
                }
            }
            openGraph.Description = HtmlHelpers.ScrubHtml(openGraph.Description);

            filterContext.Controller.ViewBag.OpenGraph = openGraph;
        }

        protected virtual Chrome PopulateChrome(HomePage homePage, SettingsBlock settings)
        {
            var chrome = new Chrome();
            chrome.TopLeftMenu = homePage.TopLeftMenu;
            chrome.TopRightMenu = homePage.TopRightMenu;
            chrome.FooterMenu = GetFooterMenuContent(homePage);
            chrome.SocialMediaIcons = homePage.SocialMediaIcons;
            chrome.LoginPage = settings.LoginPage;
            chrome.AccountPage = settings.AccountPage;
            chrome.CheckoutPage = settings.CheckoutPage;
            chrome.SearchPage = settings.SearchPage;
            if (homePage.LogoImage != null)
            {
                chrome.LogoImageUrl = _urlResolver.GetUrl(homePage.LogoImage);
            }
            else
            {
                chrome.LogoImageUrl = new Url("/Content/Images/commerce-shop-logo.png");
            }

            chrome.HomePageUrl = _urlResolver.GetUrl(homePage.ContentLink);

            // Note! The natural place for the footer content is in the settings block
            // with the rest of the content, but that makes it impossible to edit the
            // content area on the page. So we keep it directly on the start page.
            chrome.GlobalFooterContent = homePage.GlobalFooterContent;

            // Set up languages for Chrome
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var startPage = contentLoader.Get<HomePage>(ContentReference.StartPage);
            chrome.Language = startPage.LanguageBranch;
            chrome.Languages = GetLanguageInfo(startPage);
            chrome.ColorScheme = settings.Scheme;
            return chrome;
        }

        public IEnumerable<ChromeLanguageInfo> GetLanguageInfo(PageData page)
        {
            List<ChromeLanguageInfo> languages = new List<ChromeLanguageInfo>();
            ReadOnlyStringList pageLanguages = page.PageLanguages;
            string currentLanguage = page.LanguageBranch;

            foreach (string language in pageLanguages)
            {
                LanguageBranch languageBranch = _languageBranchRepository.ListEnabled().FirstOrDefault(l => l.LanguageID.Equals(language, StringComparison.InvariantCultureIgnoreCase));
                if (languageBranch != null)
                {
                    languages.Add(new ChromeLanguageInfo()
                    {
                        DisplayName = languageBranch.Name,
                        IconUrl = languageBranch.ResolvedIconPath, //"/Content/Images/flags/" + language + ".png",
                        // We use this to enable language switching inside edit mode too
                        Url = languageBranch.URLSegment,
                        EditUrl = PageEditing.GetEditUrlForLanguage(page.ContentLink, languageBranch.LanguageID),
                        Selected = string.Compare(language, currentLanguage, StringComparison.InvariantCultureIgnoreCase) == 0
                    });
                }
            }

            return languages;
        }



        private IEnumerable<PageData> GetFooterMenuContent(HomePage settings)
        {
            if (settings.FooterMenuFolder != null)
            {
                return _contentLoader.GetChildren<PageData>(settings.FooterMenuFolder).FilterForDisplay<PageData>(true, true);
            }
            else
            {
                return new List<PageData>();
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}
