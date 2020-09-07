using System;
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

        #region Get-/Setters

        public bool IsEmpty => _Segments.Count == 0;

        #endregion

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

        public PathBuilder(string path)
        {
            _Segments = new List<string>(path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            _TrailingSlash = path.EndsWith('/');
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
        /// Adds the given path to the beginning of the resulting path.
        /// </summary>
        /// <param name="path">The path to be prepended</param>
        public PathBuilder Preprend(WebPath path)
        {
            _Segments.InsertRange(0, path.Parts);
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
        /// Adds the given path to the end of the resulting path.
        /// </summary>
        /// <param name="path">The path to be appended</param>
        public PathBuilder Append(WebPath path)
        {
            _Segments.AddRange(path.Parts);
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
