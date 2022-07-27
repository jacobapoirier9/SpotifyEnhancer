/// <reference path="../lib/jquery/dist/jquery.d.ts" />
/// <reference path="../lib/jqgrid/jqGrid.d.ts" />

var actions = {
    groups: {
        open(groupId) {
            router.open("/Spotify/Groups", { groupId: groupId })
        }
    }
}

var colModels = {
    track: {
        id() { return { hidden: true, name: "Id" } as ColModelOptions },
        name() { return { name: "Name", label: "Track" } as ColModelOptions },
        image() {
            return {
                name: "Album.Images", label: "Image",
                formatter: (cellValue, info, model, action) => {
                    var html = `<img src=${cellValue[2].Url} />`
                    return html
                }
            } as ColModelOptions
        },
    },
    group: {
        groupId() { return { hidden: true, name: "GroupId" } as ColModelOptions },
        itemId() { return { hidden: true, name: "ItemId" } as ColModelOptions },
        groupName() { return { name: "GroupName", label: "Group" } as ColModelOptions },
        numberOfTracks() {
            return {
                name: "TrackCount", label: "Tracks"
            } as ColModelOptions
        },
        isMember() {
            return {
                name: "IsMember", label: "Is Member", width: 20,
                formatter: (cellValue, info, model, action) => {

                    var trackId = $("#trackId").val()

                    var cellConfig = cellValue ?
                        { icon: "fa-minus fa-lg", action: `spotify.removeTrackFromGroup('${info.rowId}', ${model.GroupId}, '${trackId}')` } :
                        { icon: "fa-plus fa-lg", action: `spotify.addTrackToGroup('${info.rowId}', ${model.GroupId}, '${trackId}')` }

                    return `<span class="toggle"><i class='fa ${cellConfig.icon}' data-next-icon="" onclick="${cellConfig.action}"></i></span>`
                }
            } as ColModelOptions
        },

        actions(options: {
            open?: boolean
        }) {
            return {
                name: "Actions", width: 20,
                formatter: (cellValue, info, model, action) => {
                    var iconStrings = ""

                    if (options.open) {
                        iconStrings += `<i class='fa fa-info' title='Open' onclick='actions.groups.open(${model.GroupId})'></i>`
                    }

                    return "<span>" + iconStrings + "</span>"
                }
            } as ColModelOptions
        }
    }
}

var spotify = {

    openTrack(trackId: string) {
        router.open("/Spotify/Track", { trackId: trackId })
    },
    openGroup(groupId: string) {
        router.open("/Spotify/Groups", { groupId: groupId })
    },

    addTrackToGroup(rowId: number, groupId: number, trackId: string) {
        $.ajax({
            url: router.route("/Spotify/AddTrackToGroup"),
            type: "POST",
            data: {
                groupId: groupId,
                trackId: trackId
            },
            success: (response) => {
                var $trackGroupsGrid = $("#trackGroupsGrid")
                $trackGroupsGrid.jqGrid("getLocalRow", rowId).IsMember = true
                $trackGroupsGrid.trigger("reloadGrid")
            }
        })
    },
    removeTrackFromGroup(rowId: number, groupId: number, trackId: string) {
        $.ajax({
            url: router.route("/Spotify/RemoveTrackFromGroup"),
            type: "POST",
            data: {
                groupId: groupId,
                trackId: trackId
            },
            success: (response) => {
                var $trackGroupsGrid = $("#trackGroupsGrid")
                $trackGroupsGrid.jqGrid("getLocalRow", rowId).IsMember = false
                $trackGroupsGrid.trigger("reloadGrid")
            }
        })
    },

    loadCurrentlyPlaying() {
        $.ajax({
            url: "/Spotify/GetCurrentlyPlaying",
            type: "POST",
            success: (response) => {

                //console.debug("CurrentlyPlaying:", response)
                var $currentlyPlaying = $("#currently-playing")

                if (response === null) {
                    //console.debug("Nothing is playing, removing image")
                    $currentlyPlaying.html("")
                }
                else if ($currentlyPlaying.attr("data-uri") === response.Item.Uri) {
                    //console.debug("Currently playing image is already set")
                } else {
                    //console.debug("Currently playing image needs to be set again")
                    $currentlyPlaying.html("")
                    $currentlyPlaying.attr("data-uri", response.Item.Uri)
                        .append("<img>").children("img")
                        .attr("src", response.Item.Album.Images[1].Url)
                        .attr("alt", response.Item.Name)
                        .attr("title", response.Item.Name)
                        .attr("width", 120)
                        .attr("height", 120)
                        .click(() => spotify.openTrack(response.Item.Id))
                }
            },
            error: (error) => {
                console.error(error)
            }
        })
    },
    page: {
        groupSingle: {
            init() {
                var $tracksGrid = $("#tracksGrid").jqGrid(helpers.createGridModel({
                    url: router.route("/Spotify/CachedTracks"),
                    colModel: [
                        colModels.track.id(),
                        colModels.track.name()
                    ]
                }))

                helpers.grid.resizeGridOnWindowResize($tracksGrid)
            }
        },
        trackSingle: {
            init() {
                var $trackGroupsGrid = $("#trackGroupsGrid").jqGrid(helpers.createGridModel({
                    url: router.route("/Spotify/CachedGroups"),
                    colModel: [
                        colModels.group.groupId(),
                        colModels.group.itemId(),
                        colModels.group.actions({ open: true }),
                        colModels.group.groupName(),
                        colModels.group.isMember() 
                    ]
                }))
                helpers.grid.setGridWidthToParentWidth($trackGroupsGrid)

                var $recommendationsGrid = $("#recommendationsGrid").jqGrid(helpers.createGridModel({
                    url: router.route("/Spotify/CachedRecommendations"),
                    colModel: [
                        colModels.track.id(),
                        colModels.track.name(),
                        colModels.track.image()
                    ]
                }))

                helpers.grid.setGridWidthToParentWidth($recommendationsGrid)
                $recommendationsGrid.jqGrid("setGridHeight", 500)
            }
        },
        groupsMultiple: {
            init() {
                var $groupsGrid = $("#groupsGrid").jqGrid(helpers.createGridModel({
                    url: router.route("/Spotify/CachedGroups"),
                    colModel: [
                        colModels.group.groupId(),
                        colModels.group.itemId(),
                        colModels.group.actions({ open: true }),
                        colModels.group.groupName(),
                        colModels.group.numberOfTracks()
                    ]
                }))

                helpers.grid.resizeGridOnWindowResize($groupsGrid)
            }
        }
    },
    init() {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval)
    }
}

