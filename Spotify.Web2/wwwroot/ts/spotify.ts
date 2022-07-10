/// <reference path="../lib/jquery/dist/jquery.d.ts" />
/// <reference path="../lib/jqgrid/jqGrid.d.ts" />

var gridModels = {
    track(override: JqGridOptions) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                { hidden: true, name: "Id", label: "TrackId" },
                { name: "Name", label: "Track"}
            ]
        })

        $.extend(model, override)
        return model
    },
    album(override: JqGridOptions) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                { hidden: true, name: "Id", label: "AlbumId" },
                { name: "Name", label: "Album" }
            ]
        })

        $.extend(model, override)
        return model
    },
    artist(override: JqGridOptions) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                { hidden: true, name: "Id", label: "ArtistId" },
                { name: "Name", label: "Artist" }
            ]
        })

        $.extend(model, override)
        return model
    },
    group(override: JqGridOptions) {
        var model = helpers.createGridModel({
            datatype: "json",
            colModel: [
                { hidden: true, name: "GroupId" },
                { hidden: true, name: "ItemId" },
                { name: "GroupName", label: "Group" },
                {
                    name: "IsMember", label: "Is Member", width: 40, align: "right",
                    formatter: (cellValue, info, model, action) => {
                        var icon = ""
                        icon = cellValue ? "fa-plus" : "fa-circle"
                        return `<span><i class='fa ${icon}'></i></span>`
                    }
                }
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
                    url: router.route("/Spotify/GetTracksFromCache"),
                    mtype: "POST"
                }))

                var $albumsGrid = $("#albumsGrid").jqGrid(gridModels.album({
                    url: router.route("/Spotify/GetAlbumsFromCache"),
                    mtype: "POST"
                }))

                var $artistsGrid = $("#artistsGrid").jqGrid(gridModels.artist({
                    url: router.route("/Spotify/GetArtistsFromCache"),
                    mtype: "POST"
                }))

                helpers.grid.resizeGridOnWindowResize($tracksGrid)
                helpers.grid.resizeGridOnWindowResize($albumsGrid)
                helpers.grid.resizeGridOnWindowResize($artistsGrid)
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

                helpers.grid.resizeGridOnWindowResize($recommendationsGrid)
            }
        },
        groupsMultiple: {
            init() {
                var $groupsGrid = $("#groupsGrid").jqGrid(gridModels.group({
                    url: router.route("/Spotify/CachedGroups"),
                    mtype: "POST"
                }))

                helpers.grid.resizeGridOnWindowResize($groupsGrid)
            }
        }
    },
    init() {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval)
    }
}

