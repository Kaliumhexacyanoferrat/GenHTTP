using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.SessionManagement;
using GenHTTP.Api.Abstraction;
using GenHTTP.Api.Abstraction.Elements;
using GenHTTP.Api.Content;
using GenHTTP.Api.Http;

namespace GenHTTP.Core.Content
{

    /// <summary>
    /// The default handler if the user has not enough rights to
    /// perform an action.
    /// </summary>
    public class DefaultNotEnoughRightsProvider : IContentProvider
    {
        private Server _Server;

        /// <summary>
        /// Create a new, default not enough rights provider.
        /// </summary>
        /// <param name="server">The related server</param>
        public DefaultNotEnoughRightsProvider(Server server)
        {
            _Server = server;
        }

        /// <summary>
        /// Will always return false.
        /// </summary>
        public bool RequiresLogin
        {
            get { return false; }
        }

        /// <summary>
        /// Will always return true.
        /// </summary>
        /// <param name="request">The request to check responsibility for</param>
        /// <param name="info">Information about the user's session</param>
        /// <returns>true</returns>
        public bool IsResponsible(IHttpRequest request, AuthorizationInfo info)
        {
            return true;
        }

        /// <summary>
        /// Print the page.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <param name="response">The response to write to</param>
        /// <param name="info">Information about the current user</param>
        public void HandleRequest(IHttpRequest request, IHttpResponse response, AuthorizationInfo info)
        {
            var page = _Server.NewPage();

            page.Title = "Error 403";
            page.Value = "You don't have the permission to perform this action.";

            response.Header.Type = ResponseType.Forbidden;

            response.Send(page);
        }

    }

}
