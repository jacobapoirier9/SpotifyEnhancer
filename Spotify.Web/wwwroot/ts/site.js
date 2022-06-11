var config = {
    loadCurrentlyPlayingInterval: 60000
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
    clickCurrentlyPlaying: function (id) {
        window.location.assign("/Spotify/TrackView?trackId=" + id);
    },
    loadCurrentlyPlaying: function () {
        $.ajax({
            url: "/Spotify/PlaybackState",
            success: function (response) {
                console.debug("Currently playing:", response);
                var $currentlyPlaying = $("#currently-playing");
                if (response === null) {
                    $currentlyPlaying.children("img").remove();
                }
                else if ($currentlyPlaying.attr("data-uri") === response.item.uri) {
                    console.debug("Currently playing is already set on the webpage");
                }
                else {
                    console.debug("Need to set currently playing on the webpage");
                    $currentlyPlaying.attr("data-uri", response.item.uri)
                        .append("<img>").children("img")
                        .attr("src", response.item.album.images[1].url)
                        .attr("alt", response.item.name)
                        .attr("title", response.item.name)
                        .attr("width", 100)
                        .attr("height", 100)
                        .click(function () { return spotify.clickCurrentlyPlaying(response.item.id); });
                }
            },
            error: function (error) {
                console.debug(error);
            }
        });
    },
    page: {
        track: {
            tabs: {
                init: function () {
                    // Each tab link will call this function, determine if the data has alread been loaded, and either load the data or just show the already loaded content
                    $("a[data-toggle='tab']").click(function (click) {
                        var $target = $(click.target);
                        //$("li").removeClass("active")
                        //$target.parent("li").addClass("active")
                        if ($target.parent("li").hasClass("disabled")) {
                            click.preventDefault();
                            return false;
                        }
                        else {
                            var $pane = $($target.attr("href"));
                            var result = $pane.attr("data-loaded");
                            if (result !== "true") {
                                switch ($pane.attr("id")) {
                                    case "tab-content-relationships":
                                        //maint.tabs.relationship.init($pane)
                                        break;
                                    case "tab-content-windows":
                                        //maint.tabs.windows.init($pane)
                                        break;
                                    case "tab-content-tidal":
                                        //maint.tabs.tidal.init($pane)
                                        break;
                                }
                                $pane.attr("data-loaded", "true");
                                //setTimeout(() => $(window).trigger("resize"), 300)
                            }
                            else {
                                //setTimeout(() => $(window).trigger("resize"), 25)
                            }
                            return true;
                        }
                    });
                }
            }
        },
        playlistBuilder: {
            init: function () {
                spotify.page.track.tabs.init();
            }
        }
    },
    init: function () {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval);
    }
};
//# sourceMappingURL=site.js.map