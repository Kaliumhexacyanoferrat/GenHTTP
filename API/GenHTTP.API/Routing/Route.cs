using System.Text;

namespace GenHTTP.Api.Routing
{

    /// <summary>
    /// Utility to work with segments within request URIs.
    /// </summary>
    public static class Route
    {

        /// <summary>
        /// Fetches the next URL segment to be handled from the
        /// given path.
        /// </summary>
        /// <param name="path">The path to chunk a segment from</param>
        /// <returns>The segment to be handled or the remaining path, if it doesn't contain an additional segment</returns>
        public static string GetSegment(string path)
        {
            var normalized = (path.StartsWith("/")) ? path.Substring(1) : path;

            var index = normalized.IndexOf('/');

            if (index > -1)
            {
                return normalized.Substring(0, index);
            }

            return normalized;
        }
        
        /// <summary>
        /// Generates an relative URI prefix to access the given level.
        /// </summary>
        /// <param name="depth">The number of levels to go up</param>
        /// <returns>The string which can be used to access the given level</returns>
        public static string GetRelation(int depth)
        {
            if (depth <= 0)
            {
                return "./";
            }

            return new StringBuilder().Insert(0, "../", depth).ToString();
        }

    }

}
