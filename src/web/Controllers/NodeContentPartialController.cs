/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Web;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Catalog;
using OxxCommerceStarterKit.Web.Models.Catalog;
using OxxCommerceStarterKit.Web.Models.FindModels;
using OxxCommerceStarterKit.Web.Models.ViewModels.Simple;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using System.Collections.Generic;
using EPiServer.Find.Api;
using EPiServer.Core;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
    [TemplateDescriptor(Inherited = true, TemplateTypeCategory = TemplateTypeCategories.MvcPartialController)]
    public class NodeContentPartialController : ContentController<NodeContent>
    {
        public string Language
        {
            get
            {
                string language = null;
                if (ControllerContext.RouteData.Values["language"] != null)
                {
                    language = ControllerContext.RouteData.Values["language"].ToString();
                }

                if (string.IsNullOrEmpty(language))
                {
                    language = ContentLanguage.PreferredCulture.Name;
                }

                return language;
            }
        }

        public void SetLanguage()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Language);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Language);
            // TODO: Inspect why we set this here, won't default work?
            ContentLanguage.Instance.SetCulture(Language);
            // Removed in CMS 9
            // EPiServer.BaseLibrary.Context.Current["EPiServer:ContentLanguage"] = new CultureInfo(Language);
        }

        public ActionResult Index(NodeContent currentContent)
        {
            SetLanguage();
            string language = Language;
            var client = SearchClient.Instance;

            IContentLoader loader = ServiceLocator.Current.GetInstance<IContentLoader>();
            ProductService productService = ServiceLocator.Current.GetInstance<ProductService>();
            ReferenceConverter refConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();


            try
            {
                SearchResults<FindProduct> results = client.Search<FindProduct>()
                    .Filter(x => x.ParentCategoryId.Match(currentContent.ContentLink.ID))
                    .Filter(x => x.Language.Match(language))
                    .StaticallyCacheFor(TimeSpan.FromMinutes(1))
                    .GetResult();

                List<ProductListViewModel> searchResult = new List<ProductListViewModel>();
                foreach (SearchHit<FindProduct> searchHit in results.Hits)
                {
                    ContentReference contentLink = refConverter.GetContentLink(searchHit.Document.Id, CatalogContentType.CatalogEntry, 0);

                    // The content can be deleted from the db, but still exist in the index
                    IContentData content = null;
                    if (loader.TryGet(contentLink, out content))
                    {
                        IProductListViewModelInitializer modelInitializer = content as IProductListViewModelInitializer;
                        if (modelInitializer != null)
                        {
                            searchResult.Add(productService.GetProductListViewModel(modelInitializer));
                        }
                    }
                }


                return PartialView("Blocks/NodeContentPartial", searchResult);
            }

            catch (Exception)
            {
                return PartialView("Blocks/NodeContentPartial", null);
            }
        }
    }
}
