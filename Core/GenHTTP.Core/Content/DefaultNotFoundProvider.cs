using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.SessionManagement;
using GenHTTP.Api.Http;
using GenHTTP.Api.Content;

namespace GenHTTP.Core.Content
{

    /// <summary>
    /// The default handler for 404 errors.
    /// </summary>
    public class DefaultNotFoundProvider : IContentProvider
    {
        private Server _Server;

        /// <summary>
        /// Create a new, default not found provider.
        /// </summary>
        /// <param name="server">The related server</param>
        public DefaultNotFoundProvider(Server server)
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
        /// Print the 404 page.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <param name="response">The response to write to</param>
        /// <param name="info">Information about the current user</param>
        public void HandleRequest(IHttpRequest request, IHttpResponse response, AuthorizationInfo info)
        {
            var page = _Server.NewPage();

            page.Title = "Error 404";
            page.Value = "The requested file ('" + request.File + "') could not be found on this server.";

            response.Header.Type = ResponseType.NotFound;

            response.Send(page);
        }

    }

}

