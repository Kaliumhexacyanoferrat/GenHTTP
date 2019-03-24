using System;
using System.Collections.Generic;
using System.Text;
using GenHTTP.Api.Content;

namespace GenHTTP.Api.Routing
{

    public class RoutingContext : IRoutingContext
    {

        #region Get-/Setters

        public IRouter Router { get; }

        public IContentProvider? ContentProvider { get; }

        #endregion

        #region Initialization

        public RoutingContext(IRouter router, IContentProvider? contentPovider = null)
        {
            Router = router;
            ContentProvider = contentPovider;
        }

        #endregion

    }

}
