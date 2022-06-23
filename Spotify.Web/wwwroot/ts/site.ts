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
                console.debug("Currently playing:", response)

                var $currentlyPlaying = $("#currently-playing")

                if (response === null) {
                    $currentlyPlaying.children("img").remove() 
                }
                else if ($currentlyPlaying.attr("data-uri") === response.Item.Uri) {
                    console.debug("Currently playing is already set on the webpage")
                } else {
                    console.debug("Need to set currently playing on the webpage")
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
                //console.debug(helpers.getJson("#relationship-json"))

                var $relationshipGrid = $("#relationship-grid").jqGrid(spotify.grid.groupRelationships.gridModel)
                helpers.grid.resizeGridOnWindowResize($relationshipGrid)

                $.ajax({
                    url: "/Spotify/GetGroupsForTrack",
                    success: (response) => {
                        console.debug("Success!", response)
                    },
                    error: (error) => {
                        console.error(error)
                    }
                })

                $relationshipGrid.trigger("reloadGrid")


                setTimeout(() => {
                    
                }, 1000)

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

    init() {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval)
    }
}

