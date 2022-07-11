/// <reference path="../lib/jquery/dist/jquery.d.ts" />
/// <reference path="../lib/jqgrid/jqGrid.d.ts" />
var actions = {
    track: {
        display: function () {
        }
    }
};
var colModels = {
    track: {
        id: function () { return { hidden: true, name: "Id" }; },
        name: function () { return { name: "Name", label: "Track" }; },
        artists: function () {
            return {};
        }
    },
    album: {
        id: function () { return { hidden: true, name: "Id" }; },
        name: function () { return { name: "Name", label: "Album" }; }
    },
    artist: {
        id: function () { return { hidden: true, name: "Id" }; },
        name: function () { return { name: "Name", label: "Artist" }; }
    },
    group: {
        groupId: function () { return { hidden: true, name: "GroupId" }; },
        itemId: function () { return { hidden: true, name: "ItemId" }; },
        groupName: function () { return { name: "GroupName", label: "Group" }; },
        numberOfTracks: function () {
            return {
                name: "TrackCount", label: "Tracks"
            };
        },
        isMember: function () {
            return {
                name: "IsMember", label: "Is Member", width: 20,
                formatter: function (cellValue, info, model, action) {
                    var icon = "";
                    icon = cellValue ? "fa-minus" : "fa-plus";
                    return "<span><i class='fa " + icon + "'></i></span>";
                }
            };
        }
    }
};
var gridModels = {
    track: function (override) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                colModels.track.id(),
                colModels.track.name()
            ]
        });
        $.extend(model, override);
        return model;
    },
    album: function (override) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                colModels.album.id(),
                colModels.album.name()
            ]
        });
        $.extend(model, override);
        return model;
    },
    artist: function (override) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                colModels.artist.id(),
                colModels.artist.name()
            ]
        });
        $.extend(model, override);
        return model;
    },
    group: function (override) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                colModels.group.groupId(),
                colModels.group.itemId(),
                colModels.group.groupName(),
                colModels.group.numberOfTracks(),
                colModels.group.isMember()
            ]
        });
        $.extend(model, override);
        return model;
    }
};
var spotify = {
    openTrack: function (trackId) {
        router.open("/Spotify/Track", { trackId: trackId });
    },
    openGroup: function (groupId) {
        router.open("/Spotify/Groups", { groupId: groupId });
    },
    loadCurrentlyPlaying: function () {
        $.ajax({
            url: "/Spotify/GetCurrentlyPlaying",
            type: "POST",
            success: function (response) {
                //console.debug("CurrentlyPlaying:", response)
                var $currentlyPlaying = $("#currently-playing");
                if (response === null) {
                    //console.debug("Nothing is playing, removing image")
                    $currentlyPlaying.html("");
                }
                else if ($currentlyPlaying.attr("data-uri") === response.Item.Uri) {
                    //console.debug("Currently playing image is already set")
                }
                else {
                    //console.debug("Currently playing image needs to be set again")
                    $currentlyPlaying.html("");
                    $currentlyPlaying.attr("data-uri", response.Item.Uri)
                        .append("<img>").children("img")
                        .attr("src", response.Item.Album.Images[1].Url)
                        .attr("alt", response.Item.Name)
                        .attr("title", response.Item.Name)
                        .attr("width", 120)
                        .attr("height", 120)
                        .click(function () { return spotify.openTrack(response.Item.Id); });
                }
            },
            error: function (error) {
                console.error(error);
            }
        });
    },
    page: {
        groupSingle: {
            init: function () {
                var $tracksGrid = $("#tracksGrid").jqGrid(gridModels.track({
                    url: router.route("/Spotify/CachedTracks"),
                    mtype: "POST"
                }));
                helpers.grid.resizeGridOnWindowResize($tracksGrid);
            }
        },
        trackSingle: {
            init: function () {
                var $trackGroupsGrid = $("#trackGroupsGrid").jqGrid(gridModels.group({
                    url: router.route("/Spotify/CachedGroups"),
                    mtype: "POST"
                }));
                helpers.grid.setGridWidthToParentWidth($trackGroupsGrid);
                var $recommendationsGrid = $("#recommendationsGrid").jqGrid(gridModels.track({
                    url: router.route("/Spotify/CachedRecommendations"),
                    mtype: "POST"
                }));
                helpers.grid.setGridWidthToParentWidth($recommendationsGrid);
                $recommendationsGrid.jqGrid("setGridHeight", 500);
            }
        },
        groupsMultiple: {
            init: function () {
                var $groupsGrid = $("#groupsGrid").jqGrid(gridModels.group({
                    url: router.route("/Spotify/CachedGroups"),
                    mtype: "POST",
                    rowNum: 20
                }));
                helpers.grid.resizeGridOnWindowResize($groupsGrid);
            }
        }
    },
    init: function () {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval);
    }
};
//# sourceMappingURL=spotify.js.map