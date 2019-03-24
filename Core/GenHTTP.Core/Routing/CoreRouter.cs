using System;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Content.Pages;

namespace GenHTTP.Core.Routing
{

    public class CoreRouter : IRouter
    {

        #region Get-/Setters

        private IServer Server { get; }

        public IRouter Router { get; }

        public IRouter Parent
        {
            get { return null; }
            set { throw new NotSupportedException("CoreRouter is intended to be the root router."); }
        }

        #endregion

        #region Initialization

        public CoreRouter(IServer server, IRouter router)
        {
            Server = server;

            Router = router;
            Router.Parent = this;
        }

        #endregion

        #region Functionality

        public void HandleContext(IEditableRoutingContext current)
        {
            Router.HandleContext(current);
        }

        public IContentPage GetPage(bool error)
        {
            return new ContentPage(Server);
        }

        public IContentProvider GetProvider(ResponseType responseType, IRoutingContext context)
        {
            var isError = (int)responseType >= 400;

            var page = context.Router.GetPage(isError);

            page.Title = responseType.ToString();
            page.Content = $"Server returned with response type '{responseType}'.";

            return new ContentPageProvider(page, responseType);
        }
        
        #endregion

    }

}
