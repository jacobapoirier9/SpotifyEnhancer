var config = {
    loadCurrentlyPlayingInterval: 50000
};
var helpers = {
    interval: {
        set: function (callback, interval) {
            callback();
            setInterval(callback, interval);
        }
    }
};
var spotify = {
    loadCurrentlyPlaying: function () {
        $.ajax({
            url: "/Spotify/PlaybackState",
            success: function (response) {
                console.debug("Currently playing:", response);
                var $currentlyPlaying = $("#currently-playing");
                if ($currentlyPlaying.attr("data-uri") === response.item.uri) {
                    console.debug("Currently playing is already set on the webpage");
                }
                else {
                    console.debug("Need to set currently playing on the webpage");
                    $currentlyPlaying.attr("data-uri", response.item.uri)
                        .children("img")
                        .attr("src", response.item.album.images[1].url)
                        .attr("alt", response.item.name)
                        .attr("title", response.item.name);
                }
            },
            error: function (error) {
                console.debug(error);
            }
        });
    },
    init: function () {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval);
    }
};
//# sourceMappingURL=site.js.map