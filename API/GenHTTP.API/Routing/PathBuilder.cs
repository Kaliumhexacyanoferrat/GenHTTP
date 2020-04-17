using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Routing
{

    /// <summary>
    /// Allows to build a <c cref="WebPath" /> instance.
    /// </summary>
    public class PathBuilder : IBuilder<WebPath>
    {
        private readonly List<string> _Segments;

        private bool _TrailingSlash;

        #region Initialization

        public PathBuilder(bool trailingSlash)
        {
            _Segments = new List<string>();
            _TrailingSlash = trailingSlash;
        }

        public PathBuilder(IEnumerable<string> parts, bool trailingSlash)
        {
            _Segments = new List<string>(parts);
            _TrailingSlash = trailingSlash;
        }

        #endregion

        #region Functionality

        /// <summary>
        /// Adds the given segment to the beginning of the resulting path.
        /// </summary>
        /// <param name="segment">The segment to be prepended</param>
        public PathBuilder Preprend(string segment)
        {
            _Segments.Insert(0, segment);
            return this;
        }

        /// <summary>
        /// Adds the given segment to the end of the resulting path.
        /// </summary>
        /// <param name="segment">The segment to be appended</param>
        public PathBuilder Append(string segment)
        {
            _Segments.Add(segment);
            return this;
        }

        /// <summary>
        /// Specifies, whether the resulting path ends with a slash or not.
        /// </summary>
        /// <param name="existent">True, if the path should end with a trailing slash</param>
        public PathBuilder TrailingSlash(bool existent)
        {
            _TrailingSlash = existent;
            return this;
        }

        public WebPath Build() => new WebPath(_Segments, _TrailingSlash);

        #endregion 

    }

}
