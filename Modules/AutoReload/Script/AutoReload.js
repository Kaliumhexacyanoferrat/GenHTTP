var genhttp = {

    interval: 2000,

    getServerVersion: (callback) => {
        var options = {
            method: "HEAD"
        };

        fetch(document.location, options).then((response) => {
            callback(response.headers.get("ETag"))
        })
    },

    checkForModifications: (originalVersion) => {
        this.getServerVersion((currentVersion) => {
            if (originalVersion !== currentVersion) {
                window.location.reload()
            }
            else {
                this.getServerVersion((version) => setTimeout(checkForModifications, this.interval, version))
            }
        })
    },

    onLoad: () => {
        this.getServerVersion((version) => setTimeout(checkForModifications, this.interval, version))
    }

};

window.addEventListener('load', genhttp.onLoad);