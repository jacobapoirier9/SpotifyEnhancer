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
            datatype: 'json',
            mtype: "JSON",
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

    icons: {
        toggle(element: HTMLElement) {
            console.debug(element)
        }
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