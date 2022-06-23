var config = {
    loadCurrentlyPlayingInterval: 60000
};
var helpers = {
    interval: {
        set: function (callback, interval) {
            callback();
            setInterval(callback, interval);
        }
    },
    getJson: function (selector) {
        return JSON.parse($(selector).val());
    },
    createGridModel: function (options) {
        var defaults = {
            datatype: 'local',
            emptyrecords: 'No records to display',
            gridview: true,
            loadonce: true,
            rowNum: 50,
            forceFit: true,
            sortable: true,
            sortorder: 'asc',
            styleUI: 'Bootstrap',
            viewrecords: true,
            jsonReader: {
                root: 'Rows',
                page: 'Page',
                total: 'Total',
                records: 'Records',
                repeatitems: false,
            }
        };
        $.extend(defaults, options);
        return defaults;
    },
    grid: {
        setGridWidthToParentWidth: function ($grid) {
            var width = Math.floor($grid.closest(".ui-jqgrid").parent().width());
            $grid.jqGrid("setGridWidth", width);
        },
        setGridHeightToViewportHeight: function ($grid) {
            // @ts-ignore
            var height = parseInt($(window).height() - $grid.offset().top - ($("header").height() || 0) - ($(".box-footer").height() || 0));
            $grid.jqGrid("setGridHeight", height);
        },
        resizeGridOnWindowResize: function ($grid) {
            $(window).on("resize expanded.pushMenu collapsed.pushMenu", function () {
                setTimeout(function () {
                    helpers.grid.setGridHeightToViewportHeight($grid);
                    helpers.grid.setGridWidthToParentWidth($grid);
                }, 350);
            }).trigger("resize");
        },
    }
};
var spotify = {
    clickCurrentlyPlaying: function (id) {
        window.location.assign("/Spotify/Track?trackId=" + id);
    },
    loadCurrentlyPlaying: function () {
        $.ajax({
            url: "/Spotify/GetCurrentlyPlaying",
            success: function (response) {
                console.debug("Currently playing:", response);
                var $currentlyPlaying = $("#currently-playing");
                if (response === null) {
                    $currentlyPlaying.children("img").remove();
                }
                else if ($currentlyPlaying.attr("data-uri") === response.Item.Uri) {
                    console.debug("Currently playing is already set on the webpage");
                }
                else {
                    console.debug("Need to set currently playing on the webpage");
                    $currentlyPlaying.attr("data-uri", response.Item.Uri)
                        .append("<img>").children("img")
                        .attr("src", response.Item.Album.Images[1].Url)
                        .attr("alt", response.Item.Name)
                        .attr("title", response.Item.Name)
                        .attr("width", 100)
                        .attr("height", 100)
                        .click(function () { return spotify.clickCurrentlyPlaying(response.Item.Id); });
                }
            },
            error: function (error) {
                console.debug(error);
            }
        });
    },
    page: {
        playlistBuilder: {
            init: function () {
            }
        },
        track: {
            init: function () {
                //console.debug(helpers.getJson("#relationship-json"))
                var $relationshipGrid = $("#relationship-grid").jqGrid(spotify.grid.groupRelationships.gridModel);
                helpers.grid.resizeGridOnWindowResize($relationshipGrid);
                $.ajax({
                    url: "/Spotify/GetGroupsForTrack",
                    success: function (response) {
                        console.debug("Success!", response);
                    },
                    error: function (error) {
                        console.error(error);
                    }
                });
                $relationshipGrid.trigger("reloadGrid");
                setTimeout(function () {
                }, 1000);
            }
        }
    },
    grid: {
        groupRelationships: {
            gridModel: helpers.createGridModel({
                datatype: "json",
                url: "/Spotify/GetGroupsForTrack",
                mtype: "POST",
                idPrefix: "rel_",
                colModel: [
                    { hidden: true, name: "ItemId" },
                    { hidden: true, name: "GroupId" },
                    { name: "GroupName", label: "Group" },
                    //{ name: "ItemType", label: "Type" },
                    { name: "AddedTo", label: "Related From" }
                ]
            }),
        }
    },
    init: function () {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval);
    }
};
//# sourceMappingURL=site.js.map