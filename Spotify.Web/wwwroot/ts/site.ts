var config = {
    loadCurrentlyPlayingInterval: 60_000
}

var helpers = {
    interval: {
        set(callback: () => void, interval: number) {
            callback()
            setInterval(callback, interval)
        }
    },

    getJson(selector: string) {
        return JSON.parse($(selector).val())
    },

    createGridModel(options: JqGridOptions): JqGridOptions {
        var defaults: JqGridOptions = {
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
        }

        $.extend(defaults, options)
        return defaults
    }
}

var spotify = {

    clickCurrentlyPlaying(id: string) {
        window.location.assign("/Spotify/TrackView?trackId=" + id)
    },

    loadCurrentlyPlaying() {
        $.ajax({
            url: "/Spotify/PlaybackState",
            success: (response) => {
                console.debug("Currently playing:", response)

                var $currentlyPlaying = $("#currently-playing")

                if (response === null) {
                    $currentlyPlaying.children("img").remove() 
                }
                else if ($currentlyPlaying.attr("data-uri") === response.item.uri) {
                    console.debug("Currently playing is already set on the webpage")
                } else {
                    console.debug("Need to set currently playing on the webpage")
                    $currentlyPlaying.attr("data-uri", response.item.uri)
                        .append("<img>").children("img")
                        .attr("src", response.item.album.images[1].url)
                        .attr("alt", response.item.name)
                        .attr("title", response.item.name)
                        .attr("width", 100)
                        .attr("height", 100)
                        .click(() => spotify.clickCurrentlyPlaying(response.item.id))
                }
            },
            error: (error) => {
                console.debug(error)
            }
        })
    },

    page: {
        playlistBuilder: {
            init() {
            }
        },
        track: {
            
            init() {
                console.debug(helpers.getJson("#relationship-json"))

                setTimeout(() => {
                    var $relationshipGrid = $("#relationship-grid").jqGrid(spotify.grid.groupRelationships.gridModel)
                    $relationshipGrid.setGridParam({ data: helpers.getJson("#relationship-json") })

                    $.ajax({
                        url: "/Spotify/GetGroupsForTrack",
                        success: (response) => {
                            console.debug("Success!", response)
                        }
                    })

                    $relationshipGrid.trigger("reloadGrid")
                }, 1000)

            }
        }
    },

    grid: {
        groupRelationships: {
            gridModel: helpers.createGridModel({
                idPrefix: "rel_",
                colModel: [
                    { hidden: true, name: "ItemId" },
                    { hidden: true, name: "GroupId" },
                    { name: "GroupName", label: "Group" },
                    { name: "ItemType", label: "Type" }
                ]

                /*
                 * GroupId: 1
GroupName: "Road Trip"
ItemId: "5J7HIH6Vf8LRbC0sgs7rV2"
ItemType: "artist"
Username: "jacobapoirier9"
                 */
            }),
        }
    },

    init() {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval)
    }
}

