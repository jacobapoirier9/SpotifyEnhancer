var spotify = {
    init: function () {
        $.ajax({
            url: "/Spotify/PlaybackState",
            success: function (response) {
                console.debug(response);
            },
            error: function (error) {
                console.debug(error);
            }
        });
    }
};
//# sourceMappingURL=site.js.map