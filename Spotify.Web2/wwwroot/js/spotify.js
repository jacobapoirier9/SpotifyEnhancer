/// <reference path="../lib/jquery/dist/jquery.d.ts" />
/// <reference path="../lib/jqgrid/jqGrid.d.ts" />
var spotify = {
    openTrack: function (trackId) {
        router.open("/Spotify/Track", { trackId: trackId });
    },
    openGroup: function (groupId) {
        router.open("/Spotify/Groups", { groupId: groupId });
    },
    loadCurrentlyPlaying: function () {
        $.ajax({
            url: "/Spotify/GetCurrentlyPlaying",
            type: "POST",
            success: function (response) {
                //console.debug("CurrentlyPlaying:", response)
                var $currentlyPlaying = $("#currently-playing");
                if (response === null) {
                    //console.debug("Nothing is playing, removing image")
                    $currentlyPlaying.html("");
                }
                else if ($currentlyPlaying.attr("data-uri") === response.Item.Uri) {
                    //console.debug("Currently playing image is already set")
                }
                else {
                    //console.debug("Currently playing image needs to be set again")
                    $currentlyPlaying.html("");
                    $currentlyPlaying.attr("data-uri", response.Item.Uri)
                        .append("<img>").children("img")
                        .attr("src", response.Item.Album.Images[1].Url)
                        .attr("alt", response.Item.Name)
                        .attr("title", response.Item.Name)
                        .attr("width", 120)
                        .attr("height", 120)
                        .click(function () { return spotify.openTrack(response.Item.Id); });
                }
            },
            error: function (error) {
                console.error(error);
            }
        });
    },
    init: function () {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval);
    }
};
//# sourceMappingURL=spotify.js.map