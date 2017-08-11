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
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Web.Mvc;
using OxxCommerceStarterKit.Web.Models.Blocks;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class PageListBlockController : BlockController<PageListBlock>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPageCriteriaQueryService _pageCriteriaQueryService;
        private readonly IContentProviderManager _providerManager;


        public PageListBlockController(IContentLoader contentLoader, IPageCriteriaQueryService pageCriteriaQueryService, IContentProviderManager providerManager)
        {
            _contentLoader = contentLoader;
            _pageCriteriaQueryService = pageCriteriaQueryService;
            _providerManager = providerManager;
        }

        public override ActionResult Index(PageListBlock currentBlock)
        {
            var startPage = _contentLoader.Get<SitePage>(ContentReference.StartPage);

            var model = CreatePageListBlockViewModel(currentBlock, startPage);

            return View(model);
        }


        public PageListBlockViewModel CreatePageListBlockViewModel(PageListBlock currentBlock, SitePage currentPage)
        {
            var pages = FindPages(currentBlock);
            var items = new List<PageListBlockItemViewModel>();

            pages = Sort(pages, currentBlock.SortOrder);

            if (currentBlock.Count > 0)
            {
                pages = pages.Take(currentBlock.Count);
                foreach (var page in pages)
                {
                    items.Add(new PageListBlockItemViewModel(page));
                }
            }

            var model = new PageListBlockViewModel(currentPage, currentBlock)
            {
                Pages = items
            };

            return model;
        }


        private IEnumerable<PageData> Sort(IEnumerable<PageData> pages, FilterSortOrder sortOrder)
        {
            var asCollection = new PageDataCollection(pages);
            var sortFilter = new FilterSort(sortOrder);
            sortFilter.Sort(asCollection);
            return asCollection;
        }

        private IEnumerable<PageData> FindPages(PageListBlock currentBlock)
        {
            IEnumerable<PageData> pages;
            var listRoot = currentBlock.Root;
            if (currentBlock.Recursive)
            {
                if (currentBlock.PageTypeFilter != null)
                {
                    pages = FindPagesByPageType(listRoot, true, currentBlock.PageTypeFilter.ID);
                }
                else
                {
                    pages = GetChildren<PageData>(listRoot);
                }
            }
            else
            {
                if (currentBlock.PageTypeFilter != null)
                {
                    pages = _contentLoader.GetChildren<PageData>(listRoot)
                        .Where(p => p.ContentTypeID == currentBlock.PageTypeFilter.ID);
                }
                else
                {
                    pages = _contentLoader.GetChildren<PageData>(listRoot);
                }
            }

            if (currentBlock.CategoryFilter != null && currentBlock.CategoryFilter.Any())
            {
                pages = pages.Where(x => x.Category.Intersect(currentBlock.CategoryFilter).Any());
            }
            return pages;
        }

        public IEnumerable<PageData> FindPagesByPageType(PageReference pageLink, bool recursive, int pageTypeId)
        {
            if (PageReference.IsNullOrEmpty(pageLink))
            {
                throw new ArgumentNullException("pageLink", "No page link specified, unable to find pages");
            }

            var pages = recursive
                        ? FindPagesByPageTypeRecursively(pageLink, pageTypeId)
                        : _contentLoader.GetChildren<PageData>(pageLink);

            return pages;
        }


        private IEnumerable<PageData> FindPagesByPageTypeRecursively(PageReference pageLink, int pageTypeId)
        {
            // TODO! Use Find instead of FindPagesWithCriteria
            var criteria = new PropertyCriteriaCollection
                               {
                                    new PropertyCriteria
                                    {
                                        Name = "PageTypeID",
                                        Type = PropertyDataType.PageType,
                                        Condition = CompareCondition.Equal,
                                        Value = pageTypeId.ToString(CultureInfo.InvariantCulture)
                                    }
                               };

            // Include content providers serving content beneath the page link specified for the search
            if (_providerManager.ProviderMap.CustomProvidersExist)
            {
                var contentProvider = _providerManager.ProviderMap.GetProvider(pageLink);

                if (contentProvider.HasCapability(ContentProviderCapabilities.Search))
                {
                    criteria.Add(new PropertyCriteria
                    {
                        Name = "EPI:MultipleSearch",
                        Value = contentProvider.ProviderKey
                    });
                }
            }

            return _pageCriteriaQueryService.FindPagesWithCriteria(pageLink, criteria);
        }


        public virtual IEnumerable<T> GetChildren<T>(ContentReference rootLink)
            where T : PageData
        {
            var children = _contentLoader.GetChildren<PageData>(rootLink);
            foreach (var child in children)
            {
                var childOfRequestedTyped = child as T;
                if (childOfRequestedTyped != null)
                {
                    yield return childOfRequestedTyped;
                }
                foreach (var descendant in GetChildren<T>(child.ContentLink))
                {
                    yield return descendant;
                }
            }
        }
    }
}
