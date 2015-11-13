/**********************************************
inRiver.ImageMap
   Version: 1.0.0 (2015-06-22)

Copyright 2015 inRiver AB

http://www.inriver.com

Dependencies:   jquery.imagemapster v1.2.10 
                jquery v2.0.3 min

***********************************************/
if (typeof window.define === "function" && window.define.amd) {
    window.define("inRiverImageMap", [], function () {
        return window.InRiverImageMap;
    });
}

window.InRiverImageMap = {
    _imageMapOptions: [],
    initImageMap: function (imageEl, options) {
        var that = this;
        imageEl.attr("inriver-id", options.imageMapData.id);
        InRiverImageMap._imageMapOptions[options.imageMapData.id] = options;
        options.keepAreasHighLighted = options.keepAreasHighLighted || false;

        // Create image map element if it doesn't exist already
        var imageMapName = "map-" + options.imageMapData.id;
        var mapEl = $("map[name^=" + imageMapName + "]");
        if (mapEl.length == 0) {
            // Insert image map to DOM
            mapEl = $("<map name='" + imageMapName + "'></map>");
            mapEl.insertAfter(imageEl);
            imageEl.attr("usemap", "#" + imageMapName);
        }

        // InRiver ImageMap options
        var imageWidth = options.imageWidth || imageEl.width();
        var imageHeight = options.imageHeight || imageEl.height();
        var spotSize = options.spotSize || 20;
        var spotImageUrl = options.spotImageUrl || null;
        var spotBackgroundColor = options.spotBackgroundColor || "#333";
        var spotForegroundColor = options.spotForegroundColor || "#fff";
        var disableSpotRendering = options.disableSpotRendering || false; // If true, spots will be treated as areas

        // Create image map areas
        this.createAreasFromData(options.imageMapData.id, imageWidth, imageHeight, spotSize);

        // The dummy area is invisible and can be useful when managing complex highlight logics (workaround for limitations in image mapster)
        if (options.useDummyArea) {
            var dummyAreaEl = $("<area/>", {
                key: options.imageMapData.id + "-dummy",
                shape: "circle",
                coords: "-10000,-10000,0",
                href: "#"
            });
            mapEl.append(dummyAreaEl);
        }

        var imageMapsterOptions = options.imageMapsterOptions || {};

        // Image mapster default values (only set if they are not supplied)
        imageMapsterOptions.isSelectable = imageMapsterOptions.isSelectable || false;
        imageMapsterOptions.mapKey = imageMapsterOptions.mapKey || "key";
        imageMapsterOptions.fillOpacity = imageMapsterOptions.fillOpacity || 0.5;
        imageMapsterOptions.fillColor = imageMapsterOptions.fillColor || "999999";
        imageMapsterOptions.stroke = imageMapsterOptions.stroke || true;
        imageMapsterOptions.strokeColor = imageMapsterOptions.strokeColor || "ffffff";
        imageMapsterOptions.strokeOpacity = imageMapsterOptions.strokeOpacity || 0.9;
        imageMapsterOptions.strokeWidth = imageMapsterOptions.strokeWidth || 1;
        imageMapsterOptions.scaleMap = imageMapsterOptions.scaleMap || false;

        $.extend(imageMapsterOptions, {
            onConfigured: function () {
                var canvas = imageEl.siblings("canvas")[0];
                var context = canvas.getContext('2d');

                // Render spots
                if (!disableSpotRendering) {
                    var spotRadius = Math.round(spotSize / 2);
                    if (spotImageUrl) {
                        var baseImage = new Image();
                        baseImage.src = spotImageUrl;
                        baseImage.onload = function () {
                            var spots = that.calculateAbsoluteSpotCoords(options.imageMapData.areas, imageWidth, imageHeight);
                            $.each(spots, function (ix, spot) {
                                context.drawImage(baseImage, spot.x - spotRadius, spot.y - spotRadius, spotRadius * 2, spotRadius * 2);
                            });
                        }
                    } else {
                        var spots = that.calculateAbsoluteSpotCoords(options.imageMapData.areas, imageWidth, imageHeight);
                        $.each(spots, function (ix, spot) {
                            context.beginPath();
                            context.arc(spot.x, spot.y, spotRadius, 0, 2 * Math.PI, false);
                            context.fillStyle = spotBackgroundColor;
                            context.fill();

                            var lineWidth = spotSize / 6;
                            var lineOffset = spotRadius * 0.7; // offset from center

                            context.beginPath();
                            context.moveTo(spot.x - lineOffset, spot.y);
                            context.lineTo(spot.x + lineOffset, spot.y);
                            context.strokeStyle = spotForegroundColor;
                            context.lineWidth = lineWidth;
                            context.stroke();

                            context.beginPath();
                            context.moveTo(spot.x, spot.y - lineOffset);
                            context.lineTo(spot.x, spot.y + lineOffset);
                            context.strokeStyle = spotForegroundColor;
                            context.lineWidth = lineWidth;
                            context.stroke();
                        });
                    }
                }

                // Callback when finished setup
                if (options.onConfigured) {
                    options.onConfigured();
                }
            },
            onClick: function (area) {
                var imageMapId = window.InRiverImageMap.getImageMapIdFromArea(area);
                var imageMapOptions = window.InRiverImageMap._imageMapOptions[imageMapId];
                if (imageMapOptions.onClick) {
                    imageMapOptions.onClick(window.InRiverImageMap.createEventDataForArea(imageMapOptions.imageMapData.areas[window.InRiverImageMap.getAreaIndexFromArea(area)]), area.e);
                }
            },
            onMouseover: function (area) {
                // Workaround of an imagemapster problem regarding highlighting
                //$('img').mapster('highlight', false);
                //$('img').mapster('highlight', area.key);

                var imageMapId = window.InRiverImageMap.getImageMapIdFromArea(area);
                var imageMapOptions = window.InRiverImageMap._imageMapOptions[imageMapId];
                if (imageMapOptions.onMouseOver) {
                    imageMapOptions.onMouseOver(window.InRiverImageMap.createEventDataForArea(imageMapOptions.imageMapData.areas[window.InRiverImageMap.getAreaIndexFromArea(area)]), area.e);
                }
            },
            onMouseout: function (area) {
                var imageMapId = window.InRiverImageMap.getImageMapIdFromArea(area);
                var imageMapOptions = window.InRiverImageMap._imageMapOptions[imageMapId];
                if (imageMapOptions.onMouseOut) {
                    imageMapOptions.onMouseOut(window.InRiverImageMap.createEventDataForArea(imageMapOptions.imageMapData.areas[window.InRiverImageMap.getAreaIndexFromArea(area)]), area.e);
                }
                if (imageMapOptions.keepAreasHighLighted) window.InRiverImageMap.highlightAllAreas(imageMapId);
            }
        });

        // Determine how spots will be rendered from an image mapster perspective
        if (!disableSpotRendering) {
            imageMapsterOptions.areas = imageMapsterOptions.areas || this.createImageMapsterAreaOptions(options.imageMapData);
        }

        imageEl.mapster(imageMapsterOptions);

        if (options.keepAreasHighLighted) this.highlightAllAreas(options.imageMapData.id);
    },
    removeImageMap: function (imageEl) {
        // Restore the image element (if image map isn't initiated, do nothing)
        var id = imageEl.attr("inriver-id");
        if (id) {
            imageEl.removeAttr("inriver-id");

            // Remove options
            InRiverImageMap._imageMapOptions.splice($.inArray(InRiverImageMap._imageMapOptions[id], InRiverImageMap._imageMapOptions), 1);

            // Unbind image mapster
            imageEl.mapster("unbind");

            // Remove image map element
            var imageMapName = "map-" + id;
            var mapEl = $("map[name^=" + imageMapName + "]");
            mapEl.remove();
        }
    },
    createEventDataForArea: function (areaData) {
        // Create object that will be sent to the callback function
        var r = {};
        r.index = areaData.index;
        if (areaData.entityId) {
            r.entityId = areaData.entityId;
        }
        return r;
    },
    calculateAbsoluteSpotCoords: function (areas, imageWidth, imageHeight) {
        var spotCoords = [];
        var spots = $.grep(areas, function (area) {
            return area.type == "spot";
        });
        $.each(spots, function (ix, spot) {
            var relativeCoords = spot.coords.split(",");
            var x = Math.round(relativeCoords[0] * imageWidth) + 0.5;
            var y = Math.round(relativeCoords[1] * imageHeight) + 0.5;
            spotCoords.push({ x: x, y: y });
        });
        return spotCoords;
    },
    createImageMapsterAreaOptions: function (imageMapData) {
        var imageMapsterAreaOptions = [];
        var spots = $.grep(imageMapData.areas, function (area) {
            return area.type == "spot";
        });
        $.each(spots, function (ix, spot) {
            imageMapsterAreaOptions.push({
                key: imageMapData.id + "-" + spot.index,
                fill: false,
                stroke: false,
                render_highlight: {
                    fill: false,
                    stroke: false
                }
            });
        });
        return imageMapsterAreaOptions;
    },
    getImageMapIdFromArea: function (area) {
        var imageMapId = area.key.split("-")[0];
        return imageMapId;
    },
    getAreaIndexFromArea: function (area) {
        var areaIndex = area.key.split("-")[1];
        return areaIndex;
    },
    createAreasFromData: function (id, imageWidth, imageHeight, spotSize) {
        var areas = InRiverImageMap._imageMapOptions[id].imageMapData.areas;
        var mapEl = $("map[name^=map-" + id + "]");
        mapEl.empty();

        $.each(areas, function (ix, area) {
            var absoluteCoords = "";
            var shape = null;

            // Transform relative coordinates
            var relativeCoords = area.coords.split(",");
            for (var i = 0; i < relativeCoords.length; i += 2) {
                absoluteCoords += Math.round(relativeCoords[i] * imageWidth) + ",";
                absoluteCoords += Math.round(relativeCoords[i + 1] * imageHeight) + ",";
            }
            absoluteCoords = absoluteCoords.substring(0, absoluteCoords.length - 1); // remove last comma

            // Special treatment for spots (they will become imagemap circles)
            if (area.type == "spot") {
                absoluteCoords += "," + (Math.round(spotSize / 2)); // radius 
                shape = "circle";
            } else {
                shape = area.type;
            }

            // Create area element
            var areaEl = $("<area/>", {
                key: id + "-" + ix,
                shape: shape,
                coords: absoluteCoords,
                href: "#"
            });
            mapEl.append(areaEl);
        });
    },
    highlightAllAreas: function (id) {
        $("area[key^=" + id + "-]").each(function () {
            $(this).mapster("highlight");
        });
    },
    removeHighlightFromAllAreas: function (id) {
        $("img[inriver-id=" + id + "]").mapster("highlight", false);
    },
    highlightArea: function (id, index) {
        $("area[key=" + id + "-" + index + "]").mapster("highlight");
    },
}