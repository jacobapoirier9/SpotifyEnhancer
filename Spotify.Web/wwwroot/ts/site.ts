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
    },
    grid: {
        setGridWidthToParentWidth: function ($grid: JQuery) {
            var width = Math.floor($grid.closest(".ui-jqgrid").parent().width());
            $grid.jqGrid("setGridWidth", width);
        },
        setGridHeightToViewportHeight: function ($grid: JQuery) {
            // @ts-ignore
            var height = parseInt($(window).height() - $grid.offset().top - ($("header").height() || 0) - ($(".box-footer").height() || 0));
            $grid.jqGrid("setGridHeight", height);
        },
        resizeGridOnWindowResize: function ($grid: JQuery) {
            $(window).on("resize expanded.pushMenu collapsed.pushMenu", function () {
                setTimeout(function () {
                    helpers.grid.setGridHeightToViewportHeight($grid);
                    helpers.grid.setGridWidthToParentWidth($grid);
                }, 350);
            }).trigger("resize");
        },
    }
}

var spotify = {

    clickCurrentlyPlaying(id: string) {
        window.location.assign("/Spotify/Track?trackId=" + id)
    },

    loadCurrentlyPlaying() {
        $.ajax({
            url: "/Spotify/GetCurrentlyPlaying",
            success: (response) => {

                console.debug("CurrentlyPlaying:", response)
                var $currentlyPlaying = $("#currently-playing")

                if (response === null) {
                    console.debug("Nothing is playing, removing image")
                    $currentlyPlaying.children("img").remove() 
                }
                else if ($currentlyPlaying.attr("data-uri") === response.Item.Uri) {
                    console.debug("Currently playing image is already set")
                } else {
                    console.debug("Currently playing image needs to be set again")
                    $currentlyPlaying.attr("data-uri", response.Item.Uri)
                        .append("<img>").children("img")
                        .attr("src", response.Item.Album.Images[1].Url)
                        .attr("alt", response.Item.Name)
                        .attr("title", response.Item.Name)
                        .attr("width", 100)
                        .attr("height", 100)
                        .click(() => spotify.clickCurrentlyPlaying(response.Item.Id))
                }
            },
            error: (error) => {
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
                var $relationshipGrid = $("#relationship-grid").jqGrid(spotify.grid.groupRelationships.gridModel)
                helpers.grid.resizeGridOnWindowResize($relationshipGrid)

                $.ajax({
                    type: "POST",
                    url: "/Spotify/GetGroupsForTrack",
                    success: (response) => {
                        console.debug("Success!", response)
                        $relationshipGrid.setGridParam({ data: response })
                        $relationshipGrid.trigger("reloadGrid")
                    },
                    error: (error) => {
                        console.error(error)
                    }
                })

            }
        },
        groups: {
            init() {

                var $groupsGrid = $("#groupsGrid").jqGrid(spotify.grid.groups.gridModel)
                helpers.grid.resizeGridOnWindowResize($groupsGrid)

                $.ajax({
                    type: "POST",
                    url: "/Spotify/GetGroups",
                    success: (response) => {
                        console.debug(response)
                        $groupsGrid.setGridParam({ data: response })
                        $groupsGrid.trigger("reloadGrid")
                    },
                    error: (error) => {
                        console.debug(error)
                    }
                })
            }
        }
    },

    grid: {
        groups: {
            gridModel: helpers.createGridModel({
                //url: "/Spotify/GetGroups",
                datatype: "local",
                //mtype: "POST",
                idPrefix: "grp_",
                colModel: [
                    { hidden: true, name: "GroupId" },
                    { name: "GroupName", label: "Group" },
                    { name: "GroupDescription", label: "Description" },
                    { name: "TrackCount", label: "Tracks" },
                    { name: "AlbumCount", label: "Albums" },
                    { name: "ArtistCount", label: "Artists" }
                ]
            })
        },
        groupRelationships: {
            gridModel: helpers.createGridModel({
                //url: "/Spotify/GetGroupsForTrack",
                datatype: "local",
                //mtype: "POST",
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

    init() {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval)
    }
}

