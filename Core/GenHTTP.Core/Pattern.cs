using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP
{

    /// <summary>
    /// Regular expressions used by this server.
    /// </summary>
    public static class Pattern
    {

        /// <summary>
        /// The format of a http request this server can handle.
        /// </summary>
        public static readonly string HTTP_REQUEST = @"^(GET|POST|HEAD) ([^ ]+) HTTP/([0-9])\.([0-9])$";

        /// <summary>
        /// A header field in a http request.
        /// </summary>
        public static readonly string HTTP_REQUEST_FIELD = @"^([0-9a-zA-Z\-]+)\:(?:[ ]*)(.*)$";

        /// <summary>
        /// A cookie definition in a header field.
        /// </summary>
        public static readonly string HTTP_REQUEST_COOKIE = @"^([^=]+)=([^;]*)";

        /// <summary>
        /// A file request relating to a project.
        /// </summary>
        public static readonly string GenHTTP_PROJECT_REQUEST = @"^/([^/]+)/";

    }

}
