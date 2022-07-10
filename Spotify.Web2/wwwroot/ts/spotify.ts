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
    //audioFeatures(override: JqGridOptions) {
    //    var model = helpers.createGridModel({
    //        datatype: "json",
    //        colModel: [
    //            { name: ""}
    //        ]
    //    })

    //    $.extend(model, override)
    //    return model
    //}
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

                var track = JSON.parse($("#trackJson").val()).Track
                console.debug("Viewing Track", track)

                var $trackGroupsGrid = $("#trackGroupsGrid").jqGrid(gridModels.group({
                    url: router.route("/Spotify/GetGroupsRelatedTo"),
                    mtype: "POST",
                    postData: {
                        id: track.id
                    }
                }))

                var $albumGroupsGrid = $("#albumGroupsGrid").jqGrid(gridModels.group({
                    url: router.route("/Spotify/GetGroupsRelatedTo"),
                    mtype: "POST",
                    postData: {
                        id: track.album.id
                    }
                }))

                helpers.grid.setGridWidthToParentWidth($trackGroupsGrid)
                helpers.grid.setGridWidthToParentWidth($albumGroupsGrid)
                //helpers.grid.resizeGridOnWindowResize($trackGroupsGrid)
                //helpers.grid.resizeGridOnWindowResize($albumGroupsGrid)
            }
        },
        groups: {
            gridModel: helpers.createGridModel({
                url: router.route("/Spotify/GetGroups"),
                mtype: "POST",
                datatype: "json",
                idPrefix: "grp_",
                colModel: [
                    { hidden: false, name: "GroupId" },
                    { name: "GroupName", label: "Group" },
                    {
                        name: "TrackCount", label: "Tracks",
                        formatter: (cellValue, info, model, action) => {
                            return `<span>${cellValue}</span><span class='pull-right' onclick='spotify.openGroup("${model.GroupId}")' style='margin: 5px;'><i class="fa fa-headphones"></i></span>`
                        }
                    },
                    { name: "AlbumCount", label: "Albums" },
                    { name: "ArtistCount", label: "Artists" }
                ]
            }),
            loadFromServer() {
                var $groupsGrid = $("#groupsGrid")
                $.ajax({
                    type: "POST",
                    url: router.route("/Spotify/GetGroups"),
                    success: (response) => {
                        $groupsGrid.setGridParam({ data: response })
                        $groupsGrid.trigger("reloadGrid")
                    }
                })
            },
            init() {
                var $groupsGrid = $("#groupsGrid").jqGrid(spotify.page.groups.gridModel)
                helpers.grid.resizeGridOnWindowResize($groupsGrid)

                helpers.modal.init("#groupModal", {
                    title: "Create Group",
                    mode: "create",
                    formData: {
                        groupName: "",
                        groupDescription: ""
                    },
                    onsubmit: {
                        create: (form) => {
                            $.ajax({
                                url: router.route("/Spotify/SaveGroup"),
                                type: "POST",
                                data: {
                                    GroupName: form.groupName,
                                    GroupDescription: form.groupDescription
                                },
                                success: (response) => {
                                    spotify.page.groups.loadFromServer()
                                }
                            })
                        }
                    }
                })
            }
        }
    },
    init() {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval)
    }
}

