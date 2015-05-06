﻿angular.module('sharedFactories').
    factory("reviewsService", [
        '$q', '$http', function($q, $http) {
        var reviewsService = {};
        var basicUrl = "/api/Review/";
        reviewsService.getReviews = function(contentid) {
            var df = $q.defer();
            var url = basicUrl + "get/?id="+contentid;
            $http.get(url).success(function (data, status, headers, config) {
                    df.resolve(data);
                })
                .error(function(data, status, headers, config) {
                    df.resolve(headers);
                });
            return df.promise;

        }

        reviewsService.postReview = function (reviewData) {
            var df = $q.defer();
            var req = {
                method: 'POST',
                url:  basicUrl + "post",
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                },
                data: reviewData
            }
            $http(req)
                .success(function (data) {
                    df.resolve(data);
                }).error(function () {
                    df.resolve(headers);
                });
            return df.promise;

          

        }

        return reviewsService;
    }
]);