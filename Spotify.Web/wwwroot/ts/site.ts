var config = {
    loadCurrentlyPlayingInterval: 50_000
}


var helpers = {
    interval: {
        set(callback: () => void, interval: number) {
            callback()
            setInterval(callback, interval)
        }
    }
}

var spotify = {

    loadCurrentlyPlaying() {
        $.ajax({
            url: "/Spotify/PlaybackState",
            success: (response) => {
                console.debug("Currently playing:", response)

                var $currentlyPlaying = $("#currently-playing")

                if ($currentlyPlaying.attr("data-uri") === response.item.uri) {
                    console.debug("Currently playing is already set on the webpage")
                } else {
                    console.debug("Need to set currently playing on the webpage")
                    $currentlyPlaying.attr("data-uri", response.item.uri)
                        .children("img")
                        .attr("src", response.item.album.images[1].url)
                        .attr("alt", response.item.name)
                        .attr("title", response.item.name)
                }
            },
            error: (error) => {
                console.debug(error)
            }
        })
    },


    init() {
        helpers.interval.set(spotify.loadCurrentlyPlaying, config.loadCurrentlyPlayingInterval)
    }
}

