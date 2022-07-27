/// <reference path="../lib/jquery/dist/jquery.d.ts" />
/// <reference path="../lib/jqgrid/jqGrid.d.ts" />
var actions = {
    groups: {
        open: function (groupId) {
            router.open("/Spotify/Groups", { groupId: groupId });
        }
    }
};
var colModels = {
    track: {
        id: function () { return { hidden: true, name: "Id" }; },
        name: function () { return { name: "Name", label: "Track" }; },
        image: function () {
            return {
                name: "Album.Images", label: "Image",
                formatter: function (cellValue, info, model, action) {
                    var html = "<img src=" + cellValue[2].Url + " />";
                    return html;
                }
            };
        },
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
                    var trackId = $("#trackId").val();
                    var cellConfig = cellValue ?
                        { icon: "fa-minus fa-lg", action: "spotify.removeTrackFromGroup('" + info.rowId + "', " + model.GroupId + ", '" + trackId + "')" } :
                        { icon: "fa-plus fa-lg", action: "spotify.addTrackToGroup('" + info.rowId + "', " + model.GroupId + ", '" + trackId + "')" };
                    return "<span class=\"toggle\"><i class='fa " + cellConfig.icon + "' data-next-icon=\"\" onclick=\"" + cellConfig.action + "\"></i></span>";
                }
            };
        },
        actions: function (options) {
            return {
                name: "Actions", width: 20,
                formatter: function (cellValue, info, model, action) {
                    var iconStrings = "";
                    if (options.open) {
                        iconStrings += "<i class='fa fa-info' title='Open' onclick='actions.groups.open(" + model.GroupId + ")'></i>";
                    }
                    return "<span>" + iconStrings + "</span>";
                }
            };
        }
    }
};
var spotify = {
    openTrack: function (trackId) {
        router.open("/Spotify/Track", { trackId: trackId });
    },
    openGroup: function (groupId) {
        router.open("/Spotify/Groups", { groupId: groupId });
    },
    addTrackToGroup: function (rowId, groupId, trackId) {
        $.ajax({
            url: router.route("/Spotify/AddTrackToGroup"),
            type: "POST",
            data: {
                groupId: groupId,
                trackId: trackId
            },
            success: function (response) {
                var $trackGroupsGrid = $("#trackGroupsGrid");
                $trackGroupsGrid.jqGrid("getLocalRow", rowId).IsMember = true;
                $trackGroupsGrid.trigger("reloadGrid");
            }
        });
    },
    removeTrackFromGroup: function (rowId, groupId, trackId) {
        $.ajax({
            url: router.route("/Spotify/RemoveTrackFromGroup"),
            type: "POST",
            data: {
                groupId: groupId,
                trackId: trackId
            },
            success: function (response) {
                var $trackGroupsGrid = $("#trackGroupsGrid");
                $trackGroupsGrid.jqGrid("getLocalRow", rowId).IsMember = false;
                $trackGroupsGrid.trigger("reloadGrid");
            }
        });
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
                var $tracksGrid = $("#tracksGrid").jqGrid(helpers.createGridModel({
                    url: router.route("/Spotify/CachedTracks"),
                    colModel: [
                        colModels.track.id(),
                        colModels.track.image(),
                        colModels.track.name()
                    ]
                }));
                helpers.grid.resizeGridOnWindowResize($tracksGrid);
            }
        },
        trackSingle: {
            init: function () {
                var $trackGroupsGrid = $("#trackGroupsGrid").jqGrid(helpers.createGridModel({
                    url: router.route("/Spotify/CachedGroups"),
                    colModel: [
                        colModels.group.groupId(),
                        colModels.group.itemId(),
                        colModels.group.actions({ open: true }),
                        colModels.group.groupName(),
                        colModels.group.isMember()
                    ]
                }));
                helpers.grid.setGridWidthToParentWidth($trackGroupsGrid);
                var $recommendationsGrid = $("#recommendationsGrid").jqGrid(helpers.createGridModel({
                    url: router.route("/Spotify/CachedRecommendations"),
                    colModel: [
                        colModels.track.id(),
                        colModels.track.name(),
                        colModels.track.image()
                    ]
                }));
                helpers.grid.setGridWidthToParentWidth($recommendationsGrid);
                $recommendationsGrid.jqGrid("setGridHeight", 500);
            }
        },
        groupsMultiple: {
            init: function () {
                var $groupsGrid = $("#groupsGrid").jqGrid(helpers.createGridModel({
                    url: router.route("/Spotify/CachedGroups"),
                    colModel: [
                        colModels.group.groupId(),
                        colModels.group.itemId(),
                        colModels.group.actions({ open: true }),
                        colModels.group.groupName(),
                        colModels.group.numberOfTracks()
                    ]
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