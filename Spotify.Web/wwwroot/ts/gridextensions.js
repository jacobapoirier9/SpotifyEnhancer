var gridExtensions = {
    SetNavGridProperties: function (grid, pager) {
        grid.jqGrid('navGrid', pager, {
            edit: false,
            add: false,
            del: false,
            refresh: false,
            search: true
        }, {}, {}, {}, {
            multipleSearch: true,
            resize: false,
            closeOnEscape: true,
            drag: true,
            Find: "Search",
            searchOnEnter: true,
            closeAfterSearch: true
        });
    },
    //EnableKeyboardNavigation: function (gridSelector) {
    //    var $grid = $(gridSelector);
    //    $grid.on("keydown", function (e) {
    //        var key = e.charCode || e.keyCode;
    //        var iRow = $grid.getGridParam('iRow');
    //        var iCol = $grid.getGridParam('iCol');
    //        function getSelectedRows() {
    //            var arr = $grid.getGridParam('selarrrow');
    //            var rows;
    //            if (arr && arr.length === 0)
    //                rows = $grid.getGridParam('selrow');
    //            else if (arr)
    //                rows = arr;
    //            return rows;
    //        };
    //        function nextCell(iCol, editableColumns) {
    //            // @ts-ignore
    //            return editableColumns[($.inArray(iCol, p = editableColumns) + 1) % editableColumns.length];
    //        }
    //        function previousCell(iCol, editableColumns) {
    //            return editableColumns[($.inArray(iCol, editableColumns) - 1 + editableColumns.length) %
    //                editableColumns.length];
    //        }
    //        function editNewCell(iRow, iCol) {
    //            e.preventDefault();
    //            e.stopPropagation();
    //            e.stopImmediatePropagation();
    //            $grid.editCell(iRow, iCol, true);
    //        };
    //        function saveCell(iRow, iCol) {
    //            e.preventDefault();
    //            e.stopPropagation();
    //            e.stopImmediatePropagation();
    //            $grid.saveCell(iRow, iCol);
    //        };
    //        if (iRow && iCol) {
    //            var editableColumns = $.map($grid.jqGrid('getGridParam', 'colModel'),
    //                function (val, i) {
    //                    return val.hidden == false && val.editable == true ? i : null;
    //                });
    //            // @ts-ignore
    //            var isShift = window.event.shiftKey ? true : false;
    //            var top = 0;
    //            var bottom = $grid.getGridParam("reccount") + 1;
    //            var left = editableColumns[0];
    //            var right = editableColumns[editableColumns.length - 1];
    //            switch (key) {
    //                case 9: // tab
    //                    if (isShift) {
    //                        if (iCol === left)
    //                            if (iRow - 1 === top)
    //                                saveCell(iRow, iCol);
    //                            else
    //                                editNewCell(iRow - 1, right);
    //                        else
    //                            editNewCell(iRow, previousCell(iCol, editableColumns));
    //                    } else {
    //                        if (iCol === right) {
    //                            if (iRow + 1 === bottom)
    //                                saveCell(iRow, iCol);
    //                            else
    //                                editNewCell(iRow + 1, left);
    //                        } else
    //                            editNewCell(iRow, nextCell(iCol, editableColumns));
    //                    }
    //                    break;
    //                case 37: // left
    //                    if (iCol === left)
    //                        saveCell(iRow, iCol);
    //                    else {
    //                        editNewCell(iRow, previousCell(iCol, editableColumns));
    //                    }
    //                    break;
    //                case 38: // up
    //                    if (iRow - 1 === top)
    //                        saveCell(iRow, iCol);
    //                    else
    //                        editNewCell(iRow - 1, iCol);
    //                    break;
    //                case 39: // right
    //                    if (iCol === right)
    //                        saveCell(iRow, iCol);
    //                    else
    //                        editNewCell(iRow, nextCell(iCol, editableColumns));
    //                    break;
    //                case 40: // down
    //                case 13: // enter
    //                    if (iRow + 1 === bottom)
    //                        saveCell(iRow, iCol);
    //                    else
    //                        editNewCell(iRow + 1, iCol);
    //                    break;
    //                case 27: // esc
    //                    $grid.restoreCell(iRow, iCol);
    //                    break;
    //            }
    //        }
    //    });
    //},
    /**
     * Sets the grid's width to the width of the parent containing element.
     * For example, if the grid table element is wrapped inside a div element,
     * the width of the grid will be set to the width of the div element.
     * @param {jQuery} $grid The grid jQuery object.
     */
    SetGridWidthToParentWidth: function ($grid) {
        // parseInt is used to discard the fractional portion of the width
        var width = parseInt($grid.closest(".ui-jqgrid").parent().width());
        $grid.jqGrid("setGridWidth", width);
    },
    /**
     * Sets the grid's height to the height of the window less the vertical space between
     * the top of the window and the top of the grid element.
     * whichever is greater.
     * @param {jQuery} $grid The grid jQuery object.
     */
    SetGridHeightToViewportHeight: function ($grid) {
        // parseInt is used to discard the fractional portion of the height
        // @ts-ignore
        var height = parseInt($(window).height() - $grid.offset().top - ($("header").height() || 0) - ($(".box-footer").height() || 0));
        $grid.jqGrid("setGridHeight", height);
    },
    /**
     * Resizes the grid (height and width) when the window is resized or when the navigation
     * sidebar is expanded or collapsed.
     * @param {jQuery} $grid The grid jQuery object.
     */
    ResizeGridOnWindowResize: function ($grid) {
        $(window).on("resize expanded.pushMenu collapsed.pushMenu", function () {
            // AdminLTE fires the pushMenu events when the sidebar begins to expand or collapse and not
            // when the operation is complete. As a result, the only way to resize the grid is to
            // wait longer than the default transaction delay of 200ms so the grid size is properly
            // calculated to the new box dimensions.
            // https://github.com/ColorlibHQ/AdminLTE/issues/1432
            setTimeout(function () {
                gridExtensions.SetGridHeightToViewportHeight($grid);
                gridExtensions.SetGridWidthToParentWidth($grid);
            }, 350);
        }).trigger("resize");
    },
    /**
     * Trigger the resize event so the grid is automatically resized correctly.
     * This must be done when the grid is in a container that is not visible when the page is initially loaded.
     * When the page is loaded, the width of the parent container is zero or negative which resizes the grid to
     * be invisible. Only once the container is shown will the width be a valid, positive number.
     */
    TriggerGridResize: function () {
        $(window).trigger("resize");
    },
    AddToggleAllSubgridsButton: function ($grid) {
        var expandAllIcon = "fa-plus-square";
        var collapseAllIcon = "fa-minus-square";
        $("#jqgh_" + $grid[0].id + "_subgrid")
            .html('<a style="cursor:pointer;" class="ui-sghref"><span class="fa ' + expandAllIcon + '" title="Expand all subgrids"></span></a>')
            .click(function () {
            var $spanIcon = $(this).find(">a>span");
            var $body = $(this).closest(".ui-jqgrid-view").find(">.ui-jqgrid-bdiv>div>.ui-jqgrid-btable>tbody");
            if ($spanIcon.hasClass(expandAllIcon)) {
                $spanIcon.removeClass(expandAllIcon)
                    .addClass(collapseAllIcon)
                    .attr("title", "Collapse all subgrids");
                $body.find(">tr.jqgrow>td.sgcollapsed").click();
            }
            else {
                $spanIcon.removeClass(collapseAllIcon)
                    .addClass(expandAllIcon)
                    .attr("title", "Expand all subgrids");
                $body.find(">tr.jqgrow>td.sgexpanded").click();
            }
        });
    }
};
//# sourceMappingURL=gridextensions.js.map