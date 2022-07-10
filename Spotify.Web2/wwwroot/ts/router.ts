var router = {
    baseUrl: "https://localhost:5001",
    init: function (baseUrl) {
        if (baseUrl.endsWith("/"))
            router.baseUrl = baseUrl.substring(0, baseUrl.length - 1)
        else
            router.baseUrl = baseUrl;

        console.debug(`Running on ${router.baseUrl}..`)
    },
    route: function (path: string, parms?: any) {
        var url = path
        if (parms != null && parms != undefined) {
            url += "?"
            for (var parm in parms) {
                url += `${parm}=${parms[parm]}`
            }
        }

        return router.baseUrl + url
    },
    open: function (path, parms?: any) {
        var openTo = path.startsWith("http") ? path : router.route(path, parms)
        window.location.assign(openTo)
    }
}