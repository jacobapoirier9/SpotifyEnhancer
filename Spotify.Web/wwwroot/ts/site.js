var config = {
    loadCurrentlyPlayingInterval: 60000
};
var helpers = {
    interval: {
        set: function (callback, interval) {
            callback();
            setInterval(callback, interval);
        }
    },
    coalesce: function () {
        var params = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            params[_i] = arguments[_i];
        }
        for (var i in params) {
            if (!helpers.isNullOfUndefined(params[i])) {
                return params[i];
            }
        }
        return null;
    },
    isNullOfUndefined: function (item) {
        return item === null || item === undefined;
    },
    getJson: function (selector) {
        return JSON.parse($(selector).val());
    },
    createGridModel: function (options) {
        var defaults = {
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
        };
        $.extend(defaults, options);
        return defaults;
    },
    grid: {
        setGridWidthToParentWidth: function ($grid) {
            var width = Math.floor($grid.closest(".ui-jqgrid").parent().width());
            $grid.jqGrid("setGridWidth", width);
        },
        setGridHeightToViewportHeight: function ($grid) {
            // @ts-ignore
            var height = parseInt($(window).height() - $grid.offset().top - ($("header").height() || 0) - ($(".box-footer").height() || 0));
            $grid.jqGrid("setGridHeight", height);
        },
        resizeGridOnWindowResize: function ($grid) {
            $(window).on("resize expanded.pushMenu collapsed.pushMenu", function () {
                setTimeout(function () {
                    helpers.grid.setGridHeightToViewportHeight($grid);
                    helpers.grid.setGridWidthToParentWidth($grid);
                }, 350);
            }).trigger("resize");
        },
    },
    modal: {
        setData: function (selector, formData) {
            var $modal = $(selector);
            // Setting form values where the key is the id and the value is translated into a dom value
            for (var key in formData) {
                var value = formData[key];
                var $item = $modal.find("#" + key);
                if ($item.attr("type") == "checkbox") {
                    $item.prop("checked", value !== null && value !== void 0 ? value : false);
                }
                else {
                    $item.val(value !== null && value !== void 0 ? value : false);
                }
            }
        },
        getData: function (selector) {
            var $modal = $(selector);
            var form = {};
            $.each($modal.find("input, select, textarea"), function (index, item) {
                var $item = $(item);
                var key = $item.attr("id");
                if (!helpers.isNullOfUndefined(key)) {
                    if ($item.attr("type") == "checkbox") {
                        console.debug("Checkbox:", $item.prop("checked"));
                        form[key] = $item.prop("checked");
                    }
                    else {
                        form[key] = $item.val();
                    }
                }
            });
            return form;
        },
        open: function (selector, options) {
            var _a;
            $("[data-target='" + selector + "']").click();
            var $modal = $(selector);
            if (!helpers.isNullOfUndefined(options.mode)) {
                $modal.attr("data-mode", options.mode);
            }
            if (!helpers.isNullOfUndefined(options.title)) {
                $modal.find(".modal-title").html(options.title);
            }
            helpers.modal.setData(selector, (_a = options.formData) !== null && _a !== void 0 ? _a : {});
        },
        init: function (selector, options) {
            var $modal = $(selector);
            // Submission
            var $submit = $modal.find("input[type=submit]");
            if (!helpers.isNullOfUndefined(options.onsubmit)) {
                $submit.show();
                $submit.click(function () {
                    var mode = $modal.attr("data-mode");
                    var form = helpers.modal.getData(selector);
                    options.onsubmit[mode](form);
                });
                if (!helpers.isNullOfUndefined(options.submitText)) {
                    $submit.val(options.submitText);
                }
            }
            else {
                $submit.hide();
            }
            // Closeout
            var $close = $modal.find("input[data-dismiss=modal]");
            if (!helpers.isNullOfUndefined(options.onclose)) {
                if (!helpers.isNullOfUndefined(options.closeText)) {
                    $close.val(options.closeText);
                }
            }
            var $button = $("[data-target='" + selector + "']");
            $button.on("click", function (click) {
                $modal.find(".modal-title").html(options.title);
                $modal.attr("data-mode", options.mode);
                // Fire the event when opening the modal
                if (!helpers.isNullOfUndefined(options.onopen)) {
                    options.onopen(click);
                }
                helpers.modal.setData(selector, options.formData);
                // Has the correct data already been loaded for this modal
                if ($modal.attr("data-loaded") === helpers.coalesce(options.name, "modal")) {
                    if (!helpers.isNullOfUndefined(options.onrefresh)) {
                        options.onrefresh(click);
                    }
                }
                else {
                    if (options.onload) {
                        options.onload(click, options.onrefresh);
                    }
                    $modal.attr("data-loaded", helpers.coalesce(options.name, "modal"));
                }
            });
        },
    }
};
var spotify = {
    openTrack: function (id) {
        window.location.assign("/Spotify/Track?trackId=" + id);
    },
    openGroup: function (groupId) {
        window.location.assign("/Spotify/Groups?groupId=" + groupId);
    },
    loadCurrentlyPlaying: function () {
        $.ajax({
            url: "/Spotify/GetCurrentlyPlaying",
            type: "POST",
            success: function (response) {
                console.debug("CurrentlyPlaying:", response);
                var $currentlyPlaying = $("#currently-playing");
                if (response === null) {
                    console.debug("Nothing is playing, removing image");
                    $currentlyPlaying.children("img").remove();
                }
                else if ($currentlyPlaying.attr("data-uri") === response.Item.Uri) {
                    console.debug("Currently playing image is already set");
                }
                else {
                    console.debug("Currently playing image needs to be set again");
                    $currentlyPlaying.attr("data-uri", response.Item.Uri)
                        .append("<img>").children("img")
                        .attr("src", response.Item.Album.Images[1].Url)
                        .attr("alt", response.Item.Name)
                        .attr("title", response.Item.Name)
                        .attr("width", 100)
                        .attr("height", 100)
                        .click(function () { return spotify.openTrack(response.Item.Id); });
                }
            },
            error: function (error) {
            }
        });
    },
    page: {
        playlistBuilder: {
            init: function () {
            }
        },
        track: {
            gridModel: helpers.createGridModel({
                datatype: "json",
                idPrefix: "rel_",
                colModel: [
                    { hidden: true, name: "ItemId" },
                    { hidden: true, name: "GroupId" },
                    { name: "GroupName", label: "Group" },
                    { name: "AddedTo", label: "Related From" }
                ]
            }),
            loadFromServer: function () {
                var $relationshipGrid = $("#relationship-grid");
                $.ajax({
                    type: "POST",
                    url: "/Spotify/GetGroupsForTrack",
                    success: function (response) {
                        console.debug("Success!", response);
                        $relationshipGrid.setGridParam({ data: response });
                        $relationshipGrid.trigger("reloadGrid");
                    },
                    error: function (error) {
                        console.error(error);
                    }
                });
            },
            init: function () {
                var $relationshipGrid = $("#relationship-grid").jqGrid(spotify.page.track.gridModel);
                helpers.grid.resizeGridOnWindowResize($relationshipGrid);
                spotify.page.track.loadFromServer();
            }
        },
        groups: {
            gridModel: helpers.createGridModel({
                url: "/Spotify/GetGroups",
                mtype: "POST",
                datatype: "json",
                idPrefix: "grp_",
                colModel: [
                    { hidden: true, name: "GroupId" },
                    { name: "GroupName", label: "Group" },
                    { name: "GroupDescription", label: "Description" },
                    {
                        name: "TrackCount", label: "Tracks",
                        formatter: function (cellValue, info, model, action) {
                            return "<span>".concat(cellValue, "</span><span class='pull-right' onclick='spotify.openGroup(\"").concat(model.GroupId, "\")' style='margin: 5px;'><i class=\"fa fa-headphones\"></i></span>");
                        }
                    },
                    { name: "AlbumCount", label: "Albums" },
                    { name: "ArtistCount", label: "Artists" }
                ]
            }),
            loadFromServer: function () {
                var $groupsGrid = $("#groupsGrid");
                $.ajax({
                    type: "POST",
                    url: "/Spotify/GetGroups",
                    success: function (response) {
                        $groupsGrid.setGridParam({ data: response });
                        $groupsGrid.trigger("reloadGrid");
                    }
                });
            },
            init: function () {
                var $groupsGrid = $("#groupsGrid").jqGrid(spotify.page.groups.gridModel);
                helpers.grid.resizeGridOnWindowResize($groupsGrid);
                helpers.modal.init("#groupModal", {
                    title: "Create Group",
                    mode: "create",
                    formData: {
                        groupName: "",
                        groupDescription: ""
                    },
                    onsubmit: {
                        create: function (form) {
                            $.ajax({
                                url: "/Spotify/SaveGroup",
                                type: "POST",
                                data: {
                                    GroupName: form.groupName,
                                    GroupDescription: form.groupDescription
                                },
                                success: function (response) {
                                    spotify.page.groups.loadFromServer();
                                }
                            });
                        }
                    }
                });
            }
        }
    },
    init: function () {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval);
    }
};
//# sourceMappingURL=site.js.map