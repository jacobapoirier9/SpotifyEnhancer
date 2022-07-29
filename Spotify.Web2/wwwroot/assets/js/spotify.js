/// <reference path="jquery.d.ts" />
/// <reference path="jqGrid.d.ts" />
var spotify = {
    page: {
        playlist: {
            init: function () {
                var $grid = $("#table").jqGrid(helpers.createGridModel({
                    url: router.route("/Home/PlaylistMultiple"),
                    colModel: [
                        { name: "Name" }
                    ]
                }));
                helpers.grid.resizeGridOnWindowResize($grid);
            },
            open: function (playlistId) {
                router.open("/Home/PlaylistSingle", { playlistId: playlistId });
            }
        }
    }
};
//# sourceMappingURL=spotify.js.map