var spotify = {
    init() {    
        $.ajax({
            url: "/Spotify/PlaybackState",
            success: (response) => {
                console.debug(response)
            },
            error: (error) => {
                console.debug(error)
            }
        })
    }
}

