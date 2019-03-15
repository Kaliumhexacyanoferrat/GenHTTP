using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP;
using GenHTTP.Content;
using GenHTTP.SessionManagement;
using GenHTTP.Abstraction;
using GenHTTP.Abstraction.Elements;

namespace GenHTTP.Content
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
        public bool IsResponsible(HttpRequest request, AuthorizationInfo info)
        {
            return true;
        }

        /// <summary>
        /// Print the 404 page.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <param name="response">The response to write to</param>
        /// <param name="info">Information about the current user</param>
        public void HandleRequest(HttpRequest request, HttpResponse response, AuthorizationInfo info)
        {
            ServerPage page = _Server.NewServerPage;

            page.Title = "Error 404";
            page.ServerVersion = _Server.Version;
            page.Value = "The requested file ('" + request.File + "') could not be found on this server.";

            response.Header.Type = ResponseType.NotFound;

            response.Send(page);
        }

    }

}

