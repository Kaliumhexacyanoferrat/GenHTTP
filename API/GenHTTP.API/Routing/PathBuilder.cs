using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Routing
{

    public class PathBuilder : IBuilder<WebPath>
    {
        private readonly List<string> _Segments = new List<string>();

        private bool _TrailingSlash;

        #region Initialization

        public PathBuilder(bool trailingSlash)
        {
            _TrailingSlash = trailingSlash;
        }

        #endregion

        #region Functionality

        public PathBuilder Preprend(string segment)
        {
            _Segments.Insert(0, segment);
            return this;
        }

        public PathBuilder Append(string segment)
        {
            _Segments.Add(segment);
            return this;
        }

        public PathBuilder TrailingSlash(bool existent)
        {
            _TrailingSlash = existent;
            return this;
        }

        public WebPath Build() => new WebPath(_Segments, _TrailingSlash);

        #endregion 

    }

}
