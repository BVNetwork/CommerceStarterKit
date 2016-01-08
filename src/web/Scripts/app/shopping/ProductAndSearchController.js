/* jshint -W099 */
/* global jQuery:false */
/* global angular:false */
(function ($, productApp) {

    "use strict";

    productApp.controller('ProductAndSearchController',['$scope', '$location', '$window', 'productService', 'articleService',
    function ($scope, $location, $window, productService, articleService) {
        $scope.selectedProductCategories = [];
      

        $scope.selectedFacets = {};

        $scope.CurrentSelectedFacetType = "";
        $scope.SelectedTerms = [];

        $scope.page = 1;
        $scope.articlePageNumber = 1;
        $scope.showMore = false;
        $scope.showMoreArticles = false;
        $scope.nrOfColumns = 3;
        $scope.pageSizeAtFiveColumns = 25;
        $scope.ShowCategoriesAsCheckboxes = false;
        $scope.ShowMainCategoryFacets = false;
        $scope.SearchDone = false;
        $scope.SearchPage = false;
        $scope.sortOrder = "popularity";

		$scope.init = function (preSelectedCategory, language, pageSize) {
		    $scope.language = language;
            if (preSelectedCategory) {
                $scope.selectedProductCategories = preSelectedCategory.split(",");
            } else {
                $scope.selectedProductCategories = [];
            }
          
            $scope.productPageSize = pageSize;
            $scope.pageSizeAtThreeColumns = pageSize;
            updateWithData();

		};
      

        $scope.initSearch = function (language, productPageSize, articlePageSize, searchedQuery) {
            $scope.SearchPage = true;
            $scope.language = language;
            $scope.productPageSize = productPageSize;
            $scope.articlePageSize = articlePageSize;
            $scope.pageSizeAtThreeColumns = productPageSize;
            if (searchedQuery) {
                $scope.queryTerm = searchedQuery;
                $scope.search();
            }
        };

        $scope.search = function () {
            setProductdata();
            $scope.loadProductData();
            $scope.loadArticles();
            $scope.SearchDone = true;
        };

        $scope.loadArticles = function () {
			$scope.showLoader();
            articleService.get($scope.queryTerm, $scope.language, $scope.articlePageNumber, $scope.articlePageSize).then(
                function (data) {
					$scope.hideLoader();
                    if ($scope.articlePageNumber === 1) {
                        $scope.articles = data.articles;
                    } else {
                        {
                            $scope.articles.push.apply($scope.articles, data.articles);
                        }
                    }
                    $scope.articlesTotalResult = data.totalResult;
                    $scope.showMoreArticles = data.totalResult > $scope.articles.length;

                }
           );

        };

        $scope.loadProductData = function () {
            $scope.showLoader();
            productService.get($scope.productData, $scope.language, $scope.page, $scope.productPageSize).then(
			  function (data) {
				  $scope.hideLoader();
			      if ($scope.page === 1) {
			          $scope.products = data.products;
			      } else {
			          {
			              $scope.products.push.apply($scope.products, data.products);
			          }
			      }
			      $scope.totalResult = data.totalResult;
			      $scope.showMore = data.totalResult > $scope.products.length;
			      $scope.productCategories = data.productCategoryFacets;
			      $scope.facets = data.facets;
			      $scope.selectedFacets = angular.copy($scope.facets);
			  },
			  function (statusCode) {
				  $scope.hideLoader();
			      $scope.status = statusCode;
			  }
		  );
           
        };

		$scope.showLoader = function() {
			$scope.loaderVisible = true;
		};

		$scope.hideLoader = function() {
			$scope.loaderVisible = false;
		};

        function updateWithData() {
            $scope.page = 1;
            setProductdata();
            $scope.loadProductData();
        }

        function setNewProductDataAndUpdate() {
            $scope.page = 1;
            setProductdata();
            $scope.loadProductData();
        }

        function setProductdata() {
            $scope.productData = {
                SearchTerm: $scope.queryTerm,
                SelectedProductCategories: $scope.selectedProductCategories,
                Facets: $scope.selectedFacets,
                SelectedFacetName: $scope.CurrentSelectedFacetType,
                SortOrder:$scope.sortOrder
            };
        }


        $scope.LoadMore = function () {
            $scope.page++;
            $scope.loadProductData();
        };

        $scope.LoadMoreArticles = function () {
            $scope.articlePageNumber++;
            $scope.loadArticles();
        };


      
        $scope.updateStringFacetsSelections = function ($event, facet, facetType) {
            var checkbox = $event.target;
            angular.forEach($scope.selectedFacets, function (facetDef, index) {
                if (facetDef.Definition.Name === facetType) {
                    //Set Current selected facet type, like country or region
                    $scope.CurrentSelectedFacetType = facetDef.Definition.Name;
                    //Update All selected Facets
                    if (checkbox.checked) {
                        facetDef.Definition.SelectedTerms.push(facet);
                    } else {
                        facetDef.Definition.SelectedTerms.splice(facetDef.Definition.SelectedTerms.indexOf(facet), 1);
                    }
                }
            });
            setNewProductDataAndUpdate();
        };

        $scope.updateRangeFacetsSelections = function ($event, facetId, facetType) {
            var selectedRange = $event.target;
            angular.forEach($scope.selectedFacets, function (facetDef, index) {
                if (facetDef.Definition.Name === facetType) {
                    //Set Current selected facet type, like country or region
                    $scope.CurrentSelectedFacetType = facetDef.Definition.Name;
                    angular.forEach(facetDef.Definition.Range, function (facetRange, i) {
                        if (facetRange.Id === facetId) {
                            facetRange.Selected = facetRange.Selected = !facetRange.Selected;
                        }
                    });
                }
            });
            setNewProductDataAndUpdate();
        };


        $scope.sortProducts = function () {
            setProductdata();
            $scope.loadProductData();
        };
      

        $scope.updateViewNrOfColumns = function ($event) {
            var $selector = $($event.currentTarget),
				mode = $selector.data('mode');
            if (mode !== $scope.nrOfColumns) {
                $scope.nrOfColumns = mode;
                $scope.productPageSize = mode === 5 ? $scope.pageSizeAtFiveColumns : $scope.pageSizeAtThreeColumns;
                if ($scope.totalResult > $scope.productPageSize) {
                    $scope.page = 1;
                    $scope.loadProductData();
                }
                else if ($scope.products.length < $scope.productPageSize) {
                    $scope.page = 1;
                    $scope.loadProductData();
                }

            }
        };

        $scope.checkAll = function (facetListToUpdate) {
            facetListToUpdate.length = 0;
            setNewProductDataAndUpdate();
        };

    }]);


})(jQuery, window.productApp);