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

        /// <summary>
        /// True, if no segments have (yet) been added to
        /// this path.
        /// </summary>
        public bool IsEmpty => _Segments.Count == 0;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new, empty path builder.
        /// </summary>
        /// <param name="trailingSlash">Whether the resulting path should end with a slash</param>
        public PathBuilder(bool trailingSlash)
        {
            _Segments = new List<string>();
            _TrailingSlash = trailingSlash;
        }

        /// <summary>
        /// Creates a new path builder with the given segments.
        /// </summary>
        /// <param name="parts">The segments of the path</param>
        /// <param name="trailingSlash">Whether the resulting path should end with a slash</param>
        public PathBuilder(IEnumerable<string> parts, bool trailingSlash)
        {
            _Segments = new List<string>(parts);
            _TrailingSlash = trailingSlash;
        }

        /// <summary>
        /// Creates a new path builder from the given absolute
        /// or relative path.
        /// </summary>
        /// <param name="path">The path to be parsed</param>
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
