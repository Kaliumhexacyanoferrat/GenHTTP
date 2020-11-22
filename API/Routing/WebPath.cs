using System;
using System.Collections.Generic;
using System.Linq;

namespace GenHTTP.Api.Routing
{

    /// <summary>
    /// Specifies a resource available on the server.
    /// </summary>
    public sealed class WebPath
    {

        #region Get-/Setters

        /// <summary>
        /// The segments the path consists of.
        /// </summary>
        public IReadOnlyList<string> Parts { get; }

        /// <summary>
        /// Specifies, whether the path ends with a trailing slash.
        /// </summary>
        public bool TrailingSlash { get; }

        /// <summary>
        /// Specifies, whether the path equals the root of the server instance.
        /// </summary>
        public bool IsRoot => (Parts.Count == 0);

        /// <summary>
        /// The name of the file that is referenced by this path (if this is
        /// the path to a file).
        /// </summary>
        public string? File
        {
            get
            {
                if (!TrailingSlash)
                {
                    var part = (Parts.Count > 0) ? Parts[Parts.Count - 1] : null;

                    return (part?.Contains('.') ?? false) ? part : null;
                }

                return null;
            }
        }

        #endregion

        #region Initialization

        public WebPath(IReadOnlyList<string> parts, bool trailingSlash)
        {
            Parts = parts;
            TrailingSlash = trailingSlash;
        }

        #endregion

        #region Functionality

        /// <summary>
        /// Creates a builder that allows to edit this path.
        /// </summary>
        /// <param name="trailingSlash">Specifies, whether the new path will have a trailing slash</param>
        /// <returns>The newly created builder instance</returns>
        public PathBuilder Edit(bool trailingSlash) => new PathBuilder(Parts, trailingSlash);

        public override string ToString() => ToString(false);

        /// <summary>
        /// Generates the string representation of this path.
        /// </summary>
        /// <param name="encoded">Specifies, whether special characters in the path should be percent encoded</param>
        /// <returns>The string representation of the path</returns>
        public string ToString(bool encoded)
        {
            if (!IsRoot)
            {
                var parts = Parts.Select(p => (encoded) ? Uri.EscapeDataString(p) : p);

                return "/" + string.Join('/', parts) + ((TrailingSlash) ? "/" : "");
            }

            return "/";
        }

        #endregion

    }

}
