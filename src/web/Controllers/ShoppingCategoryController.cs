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
using System.Text;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Framework;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Web.Mvc;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Web.Business.Recommendations;
using OxxCommerceStarterKit.Web.Business.Rss;
using OxxCommerceStarterKit.Web.Extensions;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Models.ViewModels.Simple;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class ShoppingCategoryController : PageController<ShoppingCategoryPage>
    {
        private const int DefaultNumProductsInList = 9;
        private readonly IContentLoader _contentLoader;
        private readonly IRecommendationService _recommendationService;
        private readonly ProductService _productService;

        public ShoppingCategoryController(IContentLoader contentLoader, IRecommendationService recommendationService, ProductService productService)
        {
            _contentLoader = contentLoader;
            _recommendationService = recommendationService;
            _productService = productService;
        }

        public ActionResult Index(ShoppingCategoryPage currentPage)
        {
            var model = new ShoppingCategoryViewModel(currentPage);
            model.Language = currentPage.Language.Name;

            if (currentPage.CatalogNodes != null)
            {
                model.CommerceCategoryIds = GetCommerceNodeIds(currentPage);
            }

            model.NumberOfProductsToShow = currentPage.NumberOfProductsToShow > 0 ? currentPage.NumberOfProductsToShow : DefaultNumProductsInList;

            if (currentPage.ParentLink != null)
            {
                var languageSelector = new LanguageSelector(model.Language);
                var parent = _contentLoader.Get<IContent>(currentPage.ParentLink, languageSelector);
                ShoppingCategoryPage topNode = null;

                while (parent is HomePage == false)
                {
                    topNode = parent as ShoppingCategoryPage;
                    parent = _contentLoader.Get<IContent>(parent.ParentLink, languageSelector);
                }

                if (topNode == null && parent is HomePage)
                {
                    topNode = currentPage;
                }

                model.ParentName = topNode.Name;
                model.CategoryPages = _contentLoader.GetChildren<ShoppingCategoryPage>(topNode.ContentLink);
                model.CommerceRootCategoryName = GetCommerceNodeNames(topNode);
            }

            if (!currentPage.HideRecommendations)
            {
                var node = currentPage.CatalogNodes.ToContent<NodeContent>().FirstOrDefault();
                if (node != null)
                {
                    var result = _recommendationService.GetRecommendationsForCategoryPage(node, HttpContext, currentPage);
                    model.Recommendations = CreateProductListViewModels(result,  3);
                }
            }

            model.MenuFeatureProduct = new FeatureProductViewModel(currentPage.FeatureProduct);

            return View(model);
        }

        protected List<ProductListViewModel> CreateProductListViewModels( IEnumerable<Recommendation> result, int count)
        {         
            return _productService.GetProductListViewModels(result, count).ToList();            
        }

        public ActionResult Rss(ShoppingCategoryPage currentContent)
        {

            if (Request.Url != null)
            {
                string pageBaseUrl = string.Format("{0}://{1}{2}", "https", Request.Url.Host,
                    Request.Url.IsDefaultPort ? string.Empty : ":" + Request.Url.Port);

                var result = SearchClient.Instance.Search<FindProduct>()
                    .AddFilterForIntList(GetCommerceNodeIdList(currentContent).Select(x => x.ID), "ParentCategoryId")
                    .Filter(x => x.Language.Match(currentContent.Language.TwoLetterISOLanguageName))
                    .Take(25)
                    .Select(x => new
                    {
                        x.Name,
                        Overview = x.Overview.AsCropped(250),
                        x.DefaultImageUrl,
                        x.ProductUrl
                    })
                    .GetResult();

                var items = new List<SyndicationItem>();
                foreach (var p in result)
                {
                    var productUrl = p.ProductUrl.StartsWith("https") ? p.ProductUrl : pageBaseUrl + p.ProductUrl;
                    var item = new SyndicationItem(p.Name, p.Overview, new Uri(productUrl));
                    var imageUrl = p.DefaultImageUrl.StartsWith("https")
                        ? p.DefaultImageUrl
                        : pageBaseUrl + p.DefaultImageUrl;
                    item.SetEnclosure(imageUrl + "?preset=listmedium");
                    items.Add(item);
                }

                var description = string.Empty;

                if (currentContent.PageSubHeader != null)
                {
                    description = currentContent.PageSubHeader.ToHtmlString().StripHtml();
                }

                var feed = new SyndicationFeed(currentContent.Name, description, new Uri(Request.Url.AbsoluteUri), items);

                return new FeedResult(new Rss20FeedFormatter(feed));
            }
            return null;
        }

        private string GetMainCategoryFromParentPage(ContentReference parentReference, CultureInfo languageInfo)
        {
            var parentPage = _contentLoader.Get<PageData>(parentReference, new LanguageSelector(languageInfo.Name)) as ShoppingCategoryPage;
            if (parentPage != null)
            {
                return GetCommerceNodeNames(parentPage);
            }
            return string.Empty;
        }

        private string GetCommerceNodeNames(ShoppingCategoryPage pageData)
        {
            List<string> nodeNames = new List<string>();

            // We need to load the catalog nodes, and get the name from them
            // since the link text in the CatalogNodes Link Item Collection is
            // not updated if the node changes name or code.
            // Fixes: https://github.com/OXXAS/CommerceStarterKit/issues/21
            IEnumerable<NodeContent> nodeContents = pageData.CatalogNodes.ToContent<NodeContent>();
            foreach (NodeContent nodeContent in nodeContents)
            {
                nodeNames.Add(nodeContent.Name);
            }

            return string.Join(",", nodeNames);
        }

        private List<ContentReference> GetCommerceNodeIdList(ShoppingCategoryPage pageData)
        {
            List<ContentReference> idList = new List<ContentReference>();

            if (pageData.CatalogNodes != null)
            {
                List<NodeContent> nodeContents = pageData.CatalogNodes.ToContent<NodeContent>().ToList();
                return nodeContents.GetNodeIdList();
            }
            return idList;
        }

        private string GetCommerceNodeIds(ShoppingCategoryPage pageData)
        {
            List<ContentReference> idList = GetCommerceNodeIdList(pageData);

            if (idList.Any())
            {
                string commerceCategories = string.Empty;
                foreach (ContentReference reference in idList)
                {
                    string id = reference.ID.ToString();

                    if (string.IsNullOrEmpty(commerceCategories))
                    {
                        commerceCategories = id;
                    }
                    else
                    {
                        commerceCategories += "," + id;
                    }

                }
                return commerceCategories;
            }
            return string.Empty;
        }
    }
}
