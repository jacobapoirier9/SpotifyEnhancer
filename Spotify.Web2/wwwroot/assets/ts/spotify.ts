/// <reference path="jquery.d.ts" />
/// <reference path="jqGrid.d.ts" />

var spotify = {
    page: {
        playlist: {
            init() {
                var $grid = $("#table").jqGrid(helpers.createGridModel({
                    url: router.route("/Home/PlaylistMultiple"),
                    colModel: [
                        { name: "Name"}
                    ]
                }))

                helpers.grid.resizeGridOnWindowResize($grid)
            },

            open(playlistId) {
                router.open("/Home/PlaylistSingle", { playlistId: playlistId })
            }
        }
    }
}