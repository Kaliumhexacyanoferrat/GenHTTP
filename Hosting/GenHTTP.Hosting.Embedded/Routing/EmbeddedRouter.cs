using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Hosting.Embedded.Routing
{

    internal class EmbeddedRouter : IRouter
    {

        #region Get-/Setters

        public IRouter Parent { get; set; }

        internal IRouter Router { get; }

        #endregion

        #region Initialization

        internal EmbeddedRouter(IRouter router)
        {
            Router = router;
            Router.Parent = this;
        }

        #endregion

        #region Functionality

        public void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            Router.HandleContext(current);
        }

        public IContentPage GetPage(bool error)
        {
            return Parent.GetPage(error);
        }

        public IContentProvider GetProvider(ResponseType responseType, IRoutingContext context)
        {
            return Parent.GetProvider(responseType, context);
        }

        #endregion

    }

}
