var genhttp = {

    interval: 2000,

    getServerVersion: async () => {
        var options = {
            method: "HEAD"
        };

        var response = await fetch(document.location, options)

        return response.headers.get("ETag")
    },

    checkForModifications: async (originalVersion) => {
        try {
            var currentVersion = await genhttp.getServerVersion()

            if (originalVersion !== currentVersion) {
                window.location.reload()
            }
            else {
                setTimeout(genhttp.checkForModifications, genhttp.interval, originalVersion)
            }
        }
        catch {
            setTimeout(genhttp.checkForModifications, genhttp.interval, originalVersion)
        }
    },

    onLoad: async () => {
        var version = await genhttp.getServerVersion()

        if (version && version.length > 0) {
            setTimeout(genhttp.checkForModifications, genhttp.interval, version)
        }
    }

};

window.addEventListener("load", genhttp.onLoad);