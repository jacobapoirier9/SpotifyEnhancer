function CloseWindow() {
    window.close();
}


var handheld = {
    close: function () {
        window.location.assign("/Home/Exit")
    }
}