/// <reference path="../lib/jquery/dist/jquery.d.ts" />
/// <reference path="../lib/jqgrid/jqGrid.d.ts" />

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

    coalesce(...params: any[]) {
        for (var i in params) {
            if (!helpers.isNullOfUndefined(params[i])) {
                return params[i]
            }
        }
        return null
    },

    isNullOfUndefined(item: any) {
        return item === null || item === undefined
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
    },

    modal: {
        setData(selector: string, formData: any) {
            var $modal = $(selector)
            // Setting form values where the key is the id and the value is translated into a dom value
            for (var key in formData) {
                var value = formData[key]
                var $item = $modal.find("#" + key)
                if ($item.attr("type") == "checkbox") {
                    $item.prop("checked", value ?? false)
                } else {
                    $item.val(value ?? false)
                }
            }
        },
        getData(selector: string) {
            var $modal = $(selector)
            var form = {}
            $.each($modal.find("input, select, textarea"), (index, item) => {

                var $item = $(item)
                var key = $item.attr("id")

                if (!helpers.isNullOfUndefined(key)) {
                    if ($item.attr("type") == "checkbox") {
                        console.debug("Checkbox:", $item.prop("checked"))
                        form[key] = $item.prop("checked")
                    } else {
                        form[key] = $item.val()
                    }
                }
            })

            return form
        },
        open(selector: string, options: {
            mode?: string
            formData?: any
            title?: string
        }) {
            $("[data-target='" + selector + "']").click()

            var $modal = $(selector)

            if (!helpers.isNullOfUndefined(options.mode)) {
                $modal.attr("data-mode", options.mode)
            }

            if (!helpers.isNullOfUndefined(options.title)) {
                $modal.find(".modal-title").html(options.title)
            }
            helpers.modal.setData(selector, options.formData ?? {})
        },
        init(selector: string, options: {

            title?: string
            name?: string

            onsubmit?: {
                [key: string]: (form?: any) => void
            }

            submitText?: string

            onopen?: (click?: JQueryEventObject) => void

            onclose?: () => void
            closeText?: string

            onload?: (click?: JQueryEventObject, refresh?: (click: JQueryEventObject) => void) => void
            onrefresh?: (click?: JQueryEventObject) => void

            mode?: string
            formData?: any

        }) {
            var $modal = $(selector)

            // Submission
            var $submit = $modal.find("input[type=submit]")
            if (!helpers.isNullOfUndefined(options.onsubmit)) {
                $submit.show()
                $submit.click(() => {
                    var mode = $modal.attr("data-mode")
                    var form = helpers.modal.getData(selector)
                    options.onsubmit[mode](form)
                })

                if (!helpers.isNullOfUndefined(options.submitText)) {
                    $submit.val(options.submitText)
                }
            } else {
                $submit.hide()
            }

            // Closeout
            var $close = $modal.find("input[data-dismiss=modal]")
            if (!helpers.isNullOfUndefined(options.onclose)) {
                if (!helpers.isNullOfUndefined(options.closeText)) {
                    $close.val(options.closeText)
                }
            }

            var $button = $("[data-target='" + selector + "']")
            $button.on("click", (click) => {
                $modal.find(".modal-title").html(options.title)
                $modal.attr("data-mode", options.mode)

                // Fire the event when opening the modal
                if (!helpers.isNullOfUndefined(options.onopen)) {
                    options.onopen(click)
                }

                helpers.modal.setData(selector, options.formData)

                // Has the correct data already been loaded for this modal
                if ($modal.attr("data-loaded") === helpers.coalesce(options.name, "modal")) {
                    if (!helpers.isNullOfUndefined(options.onrefresh)) {
                        options.onrefresh(click)
                    }
                }
                else {
                    if (options.onload) {
                        options.onload(click, options.onrefresh)
                    }
                    $modal.attr("data-loaded", helpers.coalesce(options.name, "modal"))
                }
            })
        },
    }
}


var router = {
    baseUrl: "https://localhost:5001",
    init: function (baseUrl) {
        if (baseUrl.endsWith("/"))
            router.baseUrl = baseUrl.substring(0, baseUrl.length - 1)
        else
            router.baseUrl = baseUrl;

        console.debug(`Running on ${router.baseUrl}..`)
    },
    route: function (path: string, parms?: any) {
        var url = path
        if (parms != null && parms != undefined) {
            url += "?"
            for (var parm in parms) {
                url += `${parm}=${parms[parm]}`
            }
        }

        return router.baseUrl + url
    },
    open: function (path, parms?: any) {
        var openTo = path.startsWith("http") ? path : router.route(path, parms)
        window.location.assign(openTo)
    }
}










var spotify = {
    openTrack(id: string) {
        router.open("/Spotify/Track", { trackId: id })
        //window.location.assign("/Spotify/Track?trackId=" + id)
    },
    openGroup(groupId: string) {
        router.open("/Spotify/Groups", { groupId: groupId })
        //window.location.assign("/Spotify/Groups?groupId=" + groupId)
    },


    loadCurrentlyPlaying() {
        $.ajax({
            url: "/Spotify/GetCurrentlyPlaying",
            type: "POST",
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
                    $currentlyPlaying.children("img").remove()
                    $currentlyPlaying.attr("data-uri", response.Item.Uri)
                        .append("<img>").children("img")
                        .attr("src", response.Item.Album.Images[1].Url)
                        .attr("alt", response.Item.Name)
                        .attr("title", response.Item.Name)
                        .attr("width", 100)
                        .attr("height", 100)
                        .click(() => spotify.openTrack(response.Item.Id))
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
            gridModel: helpers.createGridModel({
                url: router.route("/Spotify/GetGroupsForCurrentTrack"),
                mtype: "POST",
                datatype: "json",
                idPrefix: "grp_",

                colModel: [
                    { hidden: true, name: "GroupId" },
                    { name: "GroupName", label: "Group" },
                    {
                        name: "TrackCount", label: "Tracks",
                        formatter: (cellValue, info, model, action) => {
                            return `<span>${cellValue}</span><span class='pull-right' onclick='spotify.openGroup("${model.GroupId}")' style='margin: 5px;'><i class="fa fa-headphones"></i></span>`
                        }
                    },
                    { name: "AlbumCount", label: "Albums" },
                    { name: "ArtistCount", label: "Artists" }
                ],
                jsonReader: {
                    root: 'Rows',
                    page: 'Page',
                    total: 'Total',
                    records: 'Records',
                    repeatitems: false,
                    /** @ts-ignore */
                    userdata: 'UserData',
                    id: 'Id',
                    subgrid:
                    {
                        root: 'Rows',
                        repeatitems: false
                    }
                },
                subGrid: true,
                subGridOptions: {
                    expandOnLoad: false,
                    plusicon: 'fa fa-angle-right',
                    minusicon: 'fa fa-angle-down',
                    openicon: 'fa fa-bars'
                },
                subGridRowExpanded: function (subGridId, rowId) {

                    var subGridTableId = subGridId + "_t";
                    $("#" + subGridId).html("<table id='" + subGridTableId + "'></table>");
                    var $subGrid = $("#" + subGridTableId)

                    var row = $("#relationship-grid").jqGrid("getLocalRow", rowId)
                    console.debug(row)

                    $subGrid.jqGrid({
                        url: router.route("/Spotify/GetItemsForGroup"),
                        mtype: "POST",
                        datatype: "json",
                        postData: {
                            groupId: row.GroupId
                        },
                        colModel: [
                            { hidden: true, name: "Id" },
                            { name: "Name" },
                            { name: "Type" }
                        ],
                        emptyrecords: 'No records to display',
                        loadonce: true,
                        sortable: true,
                        forceFit: true,
                        //shrinkToFit: true,
                        sortorder: 'asc',
                        styleUI: 'Bootstrap',
                        viewrecords: true
                    })

                    helpers.grid.setGridWidthToParentWidth($subGrid)
                }
            }),
            init() {
                var $relationshipGrid = $("#relationship-grid").jqGrid(spotify.page.track.gridModel)
                helpers.grid.resizeGridOnWindowResize($relationshipGrid)

                //spotify.page.track.loadFromServer()
            }
        },
        groups: {
            gridModel: helpers.createGridModel({
                url: router.route("/Spotify/GetGroups"),
                mtype: "POST",
                datatype: "json",
                idPrefix: "grp_",
                colModel: [
                    { hidden: true, name: "GroupId" },
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

