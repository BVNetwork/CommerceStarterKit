/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using CsQuery.ExtensionMethods.Internal;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Api.Facets;
using EPiServer.Find.Framework;
using EPiServer.Find.Framework.Statistics;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Interfaces;
using OxxCommerceStarterKit.Web.Business.FacetRegistry;
using OxxCommerceStarterKit.Web.Extensions;
using OxxCommerceStarterKit.Web.Models.Blocks.Contracts;
using OxxCommerceStarterKit.Web.Models.FindModels;
using ReferenceConverter = Mediachase.Commerce.Catalog.ReferenceConverter;

namespace OxxCommerceStarterKit.Web.Api
{
    public class ShoppingController : BaseApiController
    {
        private readonly IRecommendedProductsService _recommendationService;
        private readonly ICurrentCustomerService _currentCustomerService;
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;
        private ILogger _log = LogManager.GetLogger();

        public ShoppingController(IRecommendedProductsService recommendationService,
            ICurrentCustomerService currentCustomerService,
            IContentLoader contentLoader,
            ReferenceConverter referenceConverter)
        {
            _recommendationService = recommendationService;
            _currentCustomerService = currentCustomerService;
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;

        }

        public class FacetViewModel
        {
            public string Name { get; set; }
            public int Count { get; set; }
            public bool Selected { get; set; }
            public string Size { get; set; }
            public string Id { get; set; }
        }

        public class SizeFacetViewModel
        {
            public string SizeUnit { get; set; }
            public bool SomeIsSelected { get; set; }
            public List<FacetViewModel> SizeFacets { get; set; }
        }


        public class ProductSearchData
        {

            [DefaultValue(1)]
            public int Page { get; set; }
            [DefaultValue(10)]
            public int PageSize { get; set; }
            public ProductData ProductData { get; set; }

        }
        public class ProductData
        {
            public List<int> SelectedProductCategories { get; set; }
            public string SearchTerm { get; set; }
            public List<FacetValues> Facets { get; set; }
            public string SelectedFacetName { get; set; }
            public string SortOrder { get; set; }
        }

        [HttpPost]
        public JObject GetProducts(ProductSearchData productSearchData)
        {
            bool enableRecommendations = false;
            if (productSearchData.ProductData.Facets.IsNullOrEmpty() && productSearchData.Page == 1)
            {
                enableRecommendations = true;
            }

            if (string.IsNullOrEmpty(productSearchData.ProductData.SearchTerm) == false)
            {
                enableRecommendations = false;
            }

            // If we do not get any facets as part of query, we use the default facets from the registry
            var facetRegistry = ServiceLocator.Current.GetInstance<FacetRegistry>();
            if (productSearchData.ProductData.Facets == null || productSearchData.ProductData.Facets.Any() == false)
            {
                productSearchData.ProductData.Facets = new List<FacetValues>();
                foreach (FacetDefinition definition in facetRegistry.FacetDefinitions)
                {
                    var valuesForFacet = new FacetValues()
                    {
                        Definition = definition
                    };
                    productSearchData.ProductData.Facets.Add(valuesForFacet);
                }
            }

            // The language is part of the route
            string language = Language;


            //Get RecommendedProducts
            List<FindProduct> recommendedFindProducts = new List<FindProduct>();
            if(enableRecommendations)
            {
                try
                {
                    recommendedFindProducts = GetRecommendedProducts(productSearchData, language);
                }
                //Recommended service is logging, so we just skip recommended products here
                catch (Exception e)
                {
                    recommendedFindProducts = new List<FindProduct>();
                }
                
            }
       
            // search term, used if part of freetext search
            var searchTerm = productSearchData.ProductData.SearchTerm;
            SearchResults<FindProduct> productsSearchResult = GetProductsForSearchAndFilters(productSearchData, searchTerm, language, recommendedFindProducts);

            string selectedFacetName = productSearchData.ProductData.SelectedFacetName ?? string.Empty;
            var productFacetsResult = GetFacetResult(productSearchData, language, productSearchData.ProductData.Facets, searchTerm);

            // parse facet results
            IEnumerable<TermCount> productCategoryFacetsResult = productFacetsResult.TermsFacetFor(x => x.ParentCategoryName).Terms;
            List<FacetViewModel> allProductCategoryFacets = CreateCategoryFacetViewModels(productCategoryFacetsResult,
                productSearchData.ProductData.SelectedProductCategories.Select(x => x.ToString()).ToList());

            // Get all facet values based on facet registry
            var facetsAndValues = GetFacetsAndValues(productFacetsResult, productSearchData.ProductData.Facets);
      
            // If we're doing a filtering on a specific facet, we handle that one
            // in a special way, in order to show the count for all terms for the facet
            SearchResults<FindProduct> productSelectedFacetsResult = null;
            if (string.IsNullOrEmpty(selectedFacetName) == false)
            {
                List<FacetValues> selectedFacet = new List<FacetValues>();
                if (productSearchData.ProductData.Facets != null)
                {
                    FacetValues selectedValues = productSearchData.ProductData.Facets.Find(f => f.Definition.Name.Equals(selectedFacetName));
                    if (selectedValues != null)
                        selectedFacet.Add(selectedValues);
                }

                // Get facet values and counts for the selected facet
                productSelectedFacetsResult = GetFacetResult(
                    productSearchData,
                    language,
                    selectedFacet,
                    searchTerm, applyFacetFilters: false);

                // Treat the selected faced specially, as it might show more data if it is selected
                facetsAndValues = GetFacetsAndValues(productSelectedFacetsResult, facetsAndValues);
            }
            List<FindProduct> productResult = new List<FindProduct>();
            if (recommendedFindProducts.Any())
            {
                productResult = recommendedFindProducts;
            }
            productResult.AddRange(productsSearchResult.ToList());

            var result = new
            {
                products = productResult,
                productCategoryFacets = allProductCategoryFacets,
                facets = facetsAndValues,
                totalResult = productsSearchResult.TotalMatching
            };

            var serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.Auto;

            return JObject.FromObject(result, serializer);

        }

        private List<FindProduct> GetRecommendedProducts(ProductSearchData productSearchData, string language)
        {
            List<FindProduct> recommendedFindProducts = new List<FindProduct>();
            CultureInfo currentCulture = new CultureInfo(language);
            List<string> categoryCodes = new List<string>();
            foreach (var categoryId in productSearchData.ProductData.SelectedProductCategories)
            {
                ContentReference catalogNodeRef = _referenceConverter.GetContentLink(categoryId, CatalogContentType.CatalogNode, 0);
                var catalogNode = _contentLoader.Get<NodeContent>(catalogNodeRef);
                if (catalogNode != null)
                {
                    categoryCodes.Add(catalogNode.Code);
                }
            }

            IEnumerable<IContent> recommendedProducts =
                    _recommendationService.GetRecommendedProductsByCagetory(_currentCustomerService.GetCurrentUserId(),
                        categoryCodes,
                        3,
                        currentCulture);

            if (recommendedProducts != null && recommendedProducts.Any())
            {
                var currentMarket = ServiceLocator.Current.GetInstance<Mediachase.Commerce.ICurrentMarket>().GetCurrentMarket();

                foreach (var product in recommendedProducts)
                {
                    if (product is IIndexableContent)
                    {
                        var productContent = product as IIndexableContent;
                        var findProduct = productContent.GetFindProduct(currentMarket);
                        recommendedFindProducts.Add(findProduct);
                    }
                }
            }
            return recommendedFindProducts;
        }

        /// <summary>
        /// Iterates the list of facets and populates the facets with the
        /// result of the facet search from Find
        /// </summary>
        /// <param name="productFacetsResult">The product facets result.</param>
        /// <param name="facetList">The facet list.</param>
        /// <returns></returns>
        private List<FacetValues> GetFacetsAndValues(SearchResults<FindProduct> productFacetsResult, List<FacetValues> facetList)
        {
            foreach (var facetValues in facetList)
            {
                FacetDefinition definition = facetValues.Definition;

                Facet facet = productFacetsResult.Facets.FirstOrDefault(f => f.Name.Equals(definition.FieldName));

                if (facet != null)
                {
                    // The definition must also keep track
                    // of what facets are selected 
                    definition.PopulateFacet(facet);
                }
            }
            return facetList;
        }

        protected SearchResults<FindProduct> GetProductsForSearchAndFilters(ProductSearchData productSearchData, string searchTerm, string language, List<FindProduct> recommendedFindProducts = null)
        {
            var productsQuery = SearchClient.Instance.Search<FindProduct>(GetFindLanguage(language));

            
            productsQuery = ApplyTermFilter(productsQuery, searchTerm, trackSearchTerm: true);

            // common filters
            productsQuery = ApplyCommonFilters(productsQuery, language);
            //Filter out recommended products
            if (recommendedFindProducts != null)
            {
                List<string> recommendedCodes = recommendedFindProducts.Select(x => x.Code).ToList();
                productsQuery = productsQuery.Filter(x => !x.Code.In(recommendedCodes));
            }
            // selected categories
            if (productSearchData.ProductData.SelectedProductCategories != null &&
                productSearchData.ProductData.SelectedProductCategories.Any())
            {
                productsQuery = productsQuery.AddFilterForIntList(productSearchData.ProductData.SelectedProductCategories, "ParentCategoryId"); 
            }

            // Add filters from the passed in fasets
            foreach (FacetValues fv in productSearchData.ProductData.Facets)
            {
                productsQuery = fv.Definition.Filter(productsQuery);
            }

            productsQuery = ApplySorting(productsQuery, productSearchData.ProductData.SortOrder);

            // execute product search
            productsQuery = productsQuery
                .Skip((productSearchData.Page - 1) * productSearchData.PageSize)
                .Take(productSearchData.PageSize)
                .StaticallyCacheFor(TimeSpan.FromMinutes(1))
                .ApplyBestBets();

            var productsSearchResult = productsQuery.GetResult();
            return productsSearchResult;
        }

        private SearchResults<FindProduct> GetFacetResult(ProductSearchData productSearchData, string language, List<FacetValues> facets, string searchTerm, bool applyFacetFilters = true)
        {
            var productFacetQuery = SearchClient.Instance.Search<FindProduct>(GetFindLanguage(language));

            // search term
            productFacetQuery = ApplyTermFilter(productFacetQuery, searchTerm);

            // common filters
            productFacetQuery = ApplyCommonFilters(productFacetQuery, language);



            // selected categories
            if (productSearchData.ProductData.SelectedProductCategories != null &&
                productSearchData.ProductData.SelectedProductCategories.Any())
            {
                productFacetQuery = productFacetQuery.AddFilterForIntList(productSearchData.ProductData.SelectedProductCategories, "ParentCategoryId"); 
            }

            productFacetQuery = productFacetQuery
                .TermsFacetFor(x => x.ParentCategoryName)
                .TermsFacetFor(x => x.MainCategoryName);

            if (facets != null && facets.Any())
            {
                foreach (FacetValues fv in facets)
                {
                    // productFacetQuery = productFacetQuery.TermsFacetFor(fv.Definition.FieldName, 50);
                    productFacetQuery = fv.Definition.Facet(productFacetQuery);
                }

                if (applyFacetFilters)
                {
                    foreach (FacetValues fv in facets)
                    {
                        productFacetQuery = fv.Definition.Filter(productFacetQuery);
                        //var selectedFacets = fv.Values.Where(x => x.Selected.Equals(true)).Select(x => x.Name).ToList();
                        //if (selectedFacets.Any())
                        //{
                        //    productFacetQuery = productFacetQuery.AddStringListFilter(selectedFacets, fv.Definition.FieldName);
                        //}
                    }

                }
            }

            var productFacetsResult = productFacetQuery
                .Take(0)
                .GetResult();
            return productFacetsResult;
        }

        private FacetValues GetFacetFromList(List<FacetValues> facets, string facetName)
        {
            if (facets == null)
                return null;
            foreach (var facetValue in facets)
            {
                if (facetValue.Definition.Name.Equals(facetName))
                {
                    return facetValue;
                }
            }
            return null;
        }


        private static ITypeSearch<FindProduct> ApplyCommonFilters(ITypeSearch<FindProduct> query, string language)
        {
            return query.Filter(x => x.Language.Match(language));

        }

        private static ITypeSearch<FindProduct> ApplySorting(ITypeSearch<FindProduct> query,string sortOrder)
        {
            if (sortOrder.Equals("popularity"))
            {
                return query.OrderByDescending(x => x.Score);
            }
            if (sortOrder.Equals("priceAscending"))
            {
                return query.OrderBy(x => x.DefaultPriceAmount);
            }
            if (sortOrder.Equals("priceDescending"))
            {
                return query.OrderByDescending(x => x.DefaultPriceAmount);
            }
            if (sortOrder.Equals("rating"))
            {
                return query.OrderByDescending(x => x.AverageRating);
            }
            return query;

        }

        /// <summary>
        /// Adds freetext search filter to known fields
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="trackSearchTerm">if set to <c>true</c> [track search term].</param>
        /// <returns></returns>
        private ITypeSearch<FindProduct> ApplyTermFilter(ITypeSearch<FindProduct> query, string searchTerm, bool trackSearchTerm = false)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.For(searchTerm)
                    .InFields(x => x.Name,
                        x => x.MainCategoryName,
                        x => string.Join(",", x.Color),
                        x => x.DisplayName,
                        x => x.Fit,
                        x => x.Description.ToString(),
                        x => string.Join(",", x.ParentCategoryName))
                        .InAllField();

                if (trackSearchTerm)
                {
                    query = query.Track();
                }
            }
            return query;
        }

        private List<SizeFacetViewModel> GetAllDifferentSizeFacets(IEnumerable<TermCount> facetResult, List<string> selectedSizes)
        {
            List<SizeFacetViewModel> allDifferentSizeFacets = new List<SizeFacetViewModel>();

            foreach (var sizeFacet in facetResult.OrderBy(x => SortSizes(x.Term)))
            {
                string term = sizeFacet.Term.ToLower();
                string[] typeAndSize = term.Split('/');
                string sizeType = typeAndSize[0];
                string size = typeAndSize[1];
                if (typeAndSize.Length == 3)
                {
                    sizeType = typeAndSize[1];
                    size = typeAndSize[2];
                }

                bool newSizeTypeList = true;
                SizeFacetViewModel sizeList = allDifferentSizeFacets.FirstOrDefault(x => x.SizeUnit == sizeType);
                if (sizeList == null)
                {
                    sizeList = new SizeFacetViewModel();
                    sizeList.SizeUnit = sizeType;
                    sizeList.SizeFacets = new List<FacetViewModel>();
                }
                else
                {
                    newSizeTypeList = false;
                }
                // check for duplicate entry and join them
                var facetViewModel = sizeList.SizeFacets.FirstOrDefault(x => x.Name == term);
                if (facetViewModel == null)
                {
                    facetViewModel = new FacetViewModel();
                    facetViewModel.Name = term;
                    facetViewModel.Size = size;
                    facetViewModel.Count = sizeFacet.Count;
                    sizeList.SizeFacets.Add(facetViewModel);
                }
                else
                {
                    facetViewModel.Count += sizeFacet.Count;
                }
                facetViewModel.Selected = selectedSizes != null && selectedSizes.Contains(term);

                if (sizeList.SizeFacets.Any(x => x.Selected.Equals(true)))
                {
                    sizeList.SomeIsSelected = true;
                }
                if (newSizeTypeList)
                {
                    allDifferentSizeFacets.Add(sizeList);
                }
            }
            return allDifferentSizeFacets;
        }

        /// <summary>
        /// Add a number in front of letter sizes to specify ordering
        /// </summary>
        /// <param name="term">Examples: unisex/xxl , 85-105, 85, inch/82-82, cm ny/20</param>
        /// <returns></returns>
        public static int SortSizes(string term)
        {
            if (term == null)
            {
                return 10000000;
            }
            term = term.ToLower();
            string sizeType = "", sizeUnit = "", size = "";
            var terms = term.Split('/');
            if (terms.Length == 2)
            {
                sizeUnit = terms[0];
                size = terms[1];
            }
            else if (terms.Length == 3)
            {
                sizeType = terms[0];
                sizeUnit = terms[1];
                size = terms[2];
            }
            else
            {
                return 10000000;
            }

            if (sizeType.StartsWith("m") &&
                sizeUnit == "standard") // herre jakke
            {
                // group sizes by ranges
                int sizeNumber = ParseInt(size, 1000000);
                if (sizeNumber == 1000000)
                {
                    return sizeNumber;
                }
                if (sizeNumber >= 46 && sizeNumber <= 64)
                {
                    return sizeNumber + 1000;
                }
                else if (sizeNumber >= 23 && sizeNumber <= 31)
                {
                    return sizeNumber + 10000;
                }
                else if (sizeNumber >= 146 && sizeNumber <= 160)
                {
                    return sizeNumber + 100000;
                }
                return sizeNumber + 1000000;
            }

            if (sizeUnit == "unisex")
            {
                var sort = LetterSizeSortIndex.blokk;
                Enum.TryParse<LetterSizeSortIndex>(size, true, out sort);
                return ((int)sort);
            }


            // check for number
            if (size.Contains("-"))
            {
                return GetSortScoreForTwoSizes(size);
            }
            else
            {
                return ParseInt(size, 100000);
            }
        }
        public static int ParseInt(string input, int defaultValue = 0)
        {
            int val = defaultValue;
            if (Int32.TryParse(input, out val))
            {
                return val;
            }
            else
            {
                return defaultValue;
            }
        }
        private static int GetSortScoreForTwoSizes(string term)
        {
            string sizePart = "";
            int sizeNumber = 0;
            sizePart = term.Split('-')[0];
            sizeNumber = ParseInt(sizePart, 100000) * 100;
            sizePart = term.Split('-')[1];
            sizeNumber += ParseInt(sizePart);
            return sizeNumber;
        }
        private enum LetterSizeSortIndex
        {
            xxxs = 10,
            xxs = 11,
            xs = 12,
            s = 13,
            m = 14,
            l = 15,
            xl = 16,
            xxl = 17,
            xxxl = 18,
            xxxxl = 19,
            xxxxxl = 20,
            xxxxxxl = 21,
            blokk = 50
        }

        private List<FacetViewModel> CreateFacetViewModels(IEnumerable<TermCount> facetResult, List<string> selectedFacets)
        {
            return facetResult.Select(
                        term =>
                            new FacetViewModel
                            {
                                Count = term.Count,
                                Name = term.Term,
                                Selected = selectedFacets != null && selectedFacets.Contains(term.Term)
                            }).ToList();
        }

        private List<FacetViewModel> CreateCategoryFacetViewModels(IEnumerable<TermCount> facetResult, List<string> selectedFacets)
        {
            List<FacetViewModel> list = new List<FacetViewModel>();

            foreach (var term in facetResult)
            {
                var id = term.Term;
                var name = term.Term;

                if (term.Term.Contains("|"))
                {
                    string[] idAndName = term.Term.Split('|');
                    id = idAndName[0];
                    name = idAndName[1];
                }

                var model = new FacetViewModel
                {
                    Count = term.Count,
                    Id = id,
                    Name = name,
                    Selected = selectedFacets != null && selectedFacets.Contains(id)
                };
                list.Add(model);
            }

            return list;
        }


        private FilterBuilder<FindProduct> GetCategoryFilter(List<int> categories)
        {
            var filter = SearchClient.Instance.BuildFilter<FindProduct>();
            // GetOrFilterForIntList
            return categories.Aggregate(filter, (current, category) => current.Or(x => x.ParentCategoryId.Match(category)));
        }
        private ITypeSearch<FindProduct> ApplyCategoryFilter(ITypeSearch<FindProduct> query, List<int> categories)
        {
            if(categories.Any())
                return query.Filter(query.GetOrFilterForIntList(categories, "ParentCategoryId", type: null)); // Filter array of int is without type specifier in Find

            return query;
        }
        private FilterBuilder<FindProduct> GetColorFilter(List<string> colorsList)
        {
            var filter = SearchClient.Instance.BuildFilter<FindProduct>();
            return colorsList.Aggregate(filter, (current, color) => current.Or(x => x.Color.Match(color)));
        }
        private FilterBuilder<FindProduct> GetSizeFilter(List<string> sizeList)
        {
            var filter = SearchClient.Instance.BuildFilter<FindProduct>();
            return sizeList.Aggregate(filter, (current, size) => current.Or(x => x.SizesList.MatchCaseInsensitive(size)));
        }
        private FilterBuilder<FindProduct> GetFitFilter(List<string> fitList)
        {
            var filter = SearchClient.Instance.BuildFilter<FindProduct>();
            return fitList.Aggregate(filter, (current, fit) => current.Or(x => x.Fit.Match(fit)));
        }
        private FilterBuilder<FindProduct> GetGrapeFilter(List<string> grapeList)
        {
            var filter = SearchClient.Instance.BuildFilter<FindProduct>();
            return grapeList.Aggregate(filter, (current, grape) => current.Or(x => x.GrapeMixList.Match(grape)));
        }

    }

    public class FacetValues
    {
        public FacetDefinition Definition { get; set; }
    }


    public class FacetValue
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public bool Selected { get; set; }
    }
}
