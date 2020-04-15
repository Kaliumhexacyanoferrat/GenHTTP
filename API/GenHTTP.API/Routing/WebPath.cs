using System.Collections.Generic;
using System.Linq;

namespace GenHTTP.Api.Routing
{

    public class WebPath
    {

        #region Get-/Setters

        public IReadOnlyList<string> Parts { get; }

        public bool TrailingSlash { get; }

        public bool IsRoot => (Parts.Count == 0);

        public string? File
        {
            get
            {
                if (!TrailingSlash)
                {
                    var part = Parts.LastOrDefault();

                    return part.Contains('.') ? part : null;
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

        public override string ToString()
        {
            if (!IsRoot)
            {
                return "/" + string.Join('/', Parts) + ((TrailingSlash) ? "/" : "");
            }

            return "/";
        }

        #endregion

    }

}
