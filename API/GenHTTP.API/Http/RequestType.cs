using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Http
{

    /// <summary>
    /// All available request types this server can handle.
    /// </summary>
    [Serializable]
    public enum RequestType
    {
        /// <summary>
        /// A http GET request.
        /// </summary>
        GET,
        /// <summary>
        /// A http HEAD request.
        /// </summary>
        HEAD,
        /// <summary>
        /// A http POST request.
        /// </summary>
        POST
    }

}
