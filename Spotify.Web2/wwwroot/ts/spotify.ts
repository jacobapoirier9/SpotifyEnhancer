/// <reference path="../lib/jquery/dist/jquery.d.ts" />
/// <reference path="../lib/jqgrid/jqGrid.d.ts" />

var actions = {
    track: {
        display() {

        }
    }
}

var colModels = {
    track: {
        id() { return { hidden: true, name: "Id" } as ColModelOptions },
        name() { return { name: "Name", label: "Track" } as ColModelOptions },
        artists() {
            return {

            } as ColModelOptions
        }
    },
    album: {
        id() { return { hidden: true, name: "Id" } as ColModelOptions },
        name() { return { name: "Name", label: "Album" } as ColModelOptions }
    },
    artist: {
        id() { return { hidden: true, name: "Id" } as ColModelOptions },
        name() { return { name: "Name", label: "Artist" } as ColModelOptions }
    },
    group: {
        groupId() { return { hidden: true, name: "GroupId" } as ColModelOptions },
        itemId() { return { hidden: true, name: "ItemId" } as ColModelOptions },
        groupName() { return { name: "GroupName", label: "Group" } as ColModelOptions },
        isMember() {
            return {
                name: "IsMember", label: "Is Member", width: 40, align: "right",
                formatter: (cellValue, info, model, action) => {
                    var icon = ""
                    icon = cellValue ? "fa-plus" : "fa-circle"
                    return `<span><i class='fa ${icon}'></i></span>`
                }
            } as ColModelOptions
        }
    }
}

var gridModels = {
    track(override: JqGridOptions) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                colModels.track.id(),
                colModels.track.name()
            ]
        })

        $.extend(model, override)
        return model
    },
    album(override: JqGridOptions) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                colModels.album.id(),
                colModels.album.name()
            ]
        })

        $.extend(model, override)
        return model
    },
    artist(override: JqGridOptions) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                colModels.artist.id(),
                colModels.artist.name()
            ]
        })

        $.extend(model, override)
        return model
    },
    group(override: JqGridOptions) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                colModels.group.groupId(),
                colModels.group.itemId(),
                colModels.group.groupName(),
                colModels.group.isMember()
            ]
        })

        $.extend(model, override)
        return model
    }
}


var spotify = {

    openTrack(trackId: string) {
        router.open("/Spotify/Track", { trackId: trackId })
    },
    openGroup(groupId: string) {
        router.open("/Spotify/Groups", { groupId: groupId })
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
                var $tracksGrid = $("#tracksGrid").jqGrid(gridModels.track({
                    url: router.route("/Spotify/CachedTracks"),
                    mtype: "POST"
                }))

                helpers.grid.resizeGridOnWindowResize($tracksGrid)
            }
        },
        trackSingle: {
            init() {
                var $trackGroupsGrid = $("#trackGroupsGrid").jqGrid(gridModels.group({
                    url: router.route("/Spotify/CachedGroups"),
                    mtype: "POST"
                }))

                helpers.grid.setGridWidthToParentWidth($trackGroupsGrid)

                var $recommendationsGrid = $("#recommendationsGrid").jqGrid(gridModels.track({
                    url: router.route("/Spotify/GetRecommendations"),
                    mtype: "POST"
                }))

                helpers.grid.setGridWidthToParentWidth($recommendationsGrid)
                $recommendationsGrid.jqGrid("setGridHeight", 500)
            }
        },
        groupsMultiple: {
            init() {
                var $groupsGrid = $("#groupsGrid").jqGrid(gridModels.group({
                    url: router.route("/Spotify/CachedGroups"),
                    mtype: "POST",
                    rowNum: 20
                }))

                helpers.grid.resizeGridOnWindowResize($groupsGrid)
            }
        }
    },
    init() {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval)
    }
}

