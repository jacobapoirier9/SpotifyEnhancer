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
            mtype: "POST",
            datatype: 'json',
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
    }
};
var spotify = {
    clickCurrentlyPlaying: function (id) {
        window.location.assign("/Spotify/TrackView?trackId=" + id);
    },
    loadCurrentlyPlaying: function () {
        $.ajax({
            url: "/Spotify/PlaybackState",
            success: function (response) {
                console.debug("Currently playing:", response);
                var $currentlyPlaying = $("#currently-playing");
                if (response === null) {
                    $currentlyPlaying.children("img").remove();
                }
                else if ($currentlyPlaying.attr("data-uri") === response.item.uri) {
                    console.debug("Currently playing is already set on the webpage");
                }
                else {
                    console.debug("Need to set currently playing on the webpage");
                    $currentlyPlaying.attr("data-uri", response.item.uri)
                        .append("<img>").children("img")
                        .attr("src", response.item.album.images[1].url)
                        .attr("alt", response.item.name)
                        .attr("title", response.item.name)
                        .attr("width", 100)
                        .attr("height", 100)
                        .click(function () { return spotify.clickCurrentlyPlaying(response.item.id); });
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
                console.debug(helpers.getJson("#relationship-json"));
                setTimeout(function () {
                    var $relationshipGrid = $("#relationship-grid").jqGrid(spotify.grid.groupRelationships.gridModel);
                }, 1000);
            }
        }
    },
    grid: {
        groupRelationships: {
            gridModel: helpers.createGridModel({
                idPrefix: "rel_",
                colModel: [
                    {}
                ]
            }),
        }
    },
    init: function () {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval);
    }
};
//# sourceMappingURL=site.js.map