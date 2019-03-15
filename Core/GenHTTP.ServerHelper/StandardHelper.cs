using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using GenHTTP.Api.Abstraction;
using GenHTTP.Api.Abstraction.Elements;
using GenHTTP.Api.Abstraction.Style;
using GenHTTP.Api.Project;
using GenHTTP.Api.Http;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Core.ServerHelper
{

    /// <summary>
    /// Supports the server by generating some standard pages (index, 404, 500, ...).
    /// </summary>
    public class StandardHelper : IServerHelper
    {
        private IServer _Server;
        private DocumentType _Type = DocumentType.XHtml_1_1_Strict;
        private IContentProvider _NotFound;

        /// <summary>
        /// Initializes this helper object.
        /// </summary>
        /// <param name="server">The server this helper is assigned to</param>
        public void Init(IServer server)
        {
            _Server = server;
            _NotFound = server.DefaultNotFoundProvider;
        }

        /// <summary>
        /// Generate the index page for the server.
        /// </summary>
        /// <param name="request">The request of the client</param>
        /// <param name="response">The response to write to</param>
        public void GenerateIndex(IHttpRequest request, IHttpResponse response)
        {
            IServerPage page = _Server.NewPage();

            page.Title = "Server index";

            // write list
            NeutralElement list = new NeutralElement();
            list.AddText("Available projects on this server:");
            list.AddNewLine(_Type);
            foreach (IProject project in _Server.Projects)
            {
                list.AddNewLine(_Type);
                Link link = list.AddLink("/" + project.Name + "/", project.Name + ", v" + project.Version);
                link.Color = ElementColor.Black;
            }
            list.AddNewLine(_Type, 2);

            page.Value = list.Serialize(_Type);

            // log that we sent the index page
            _Server.Log.WriteLine("Sent index to client");
            // send the page
            response.Send(page);
        }

        /// <summary>
        /// Generate a 404 error message.
        /// </summary>
        /// <param name="request">The request of the client</param>
        /// <param name="response">The response to write to</param>
        public void GenerateNotFound(IHttpRequest request, IHttpResponse response)
        {
            _NotFound.HandleRequest(request, response, null);
            // log this request
            _Server.Log.WriteLine("File not found: " + request.File);
        }

        /// <summary>
        /// Generate a 500 error message (internal server error).
        /// </summary>
        /// <param name="request">The request of the client</param>
        /// <param name="response">The response to write to</param>
        /// <param name="exception">The exception which occured while running the project's method</param>
        public void GenerateServerError(IHttpRequest request, IHttpResponse response, Exception exception)
        {
            var page = _Server.NewPage();
            // prepare response
            response.Header.Type = ResponseType.InternalServerError;

            page.Title = "Error 500";
            page.Value = "The server could not handle your request (<b>" + request.File + "</b>).";

            // log this request
            _Server.Log.WriteLine("Error handling '" + request.File + "': " + exception.Message);
            _Server.Log.WriteLine(exception.StackTrace);
            _Server.Log.WriteLine(exception.InnerException.Message);
            // and send the response
            response.Send(page);
        }

        /// <summary>
        /// Generate a error message, which tells the user, that the
        /// project did not send any response.
        /// </summary>
        /// <param name="request">The request of the client</param>
        /// <param name="response">The response to write to</param>
        public void GenerateNoContent(IHttpRequest request, IHttpResponse response)
        {
            var page = _Server.NewPage();
            // prepare response
            response.Header.Type = ResponseType.InternalServerError;

            page.Title = "Error 500";
            page.Value = "The responsible project did not handle this request.";

            // log this request
            _Server.Log.WriteLine("Project failed to handle request '" + request.File + "'");
            // and send this response
            response.Send(page);
        }

        /// <summary>
        /// Implement some additional features (like needed stylesheets, favicon or robots.txt).
        /// </summary>
        /// <param name="request">The request of the client</param>
        /// <param name="response">The response to write to</param>
        /// <returns>true, if this method handled this request</returns>
        public bool GenerateOther(IHttpRequest request, IHttpResponse response)
        {
            // support all additional files, stylesheets etc.
            string file = request.File;
            if (request.Project != null) request.File.Replace("/" + request.Project.Name, ""); // due to virtual hosting
            if (File.Exists(_Server.Path + "common" + file))
            {
                Download download = new Download(_Server.Path + "common" + file);
                response.Send(download);
                _Server.Log.WriteLine("Sent '" + file + "' to client");
                return true;
            }
            return false;
        }

        /// <summary>
        /// This method will be called whenever the server recieved a malformed request.
        /// </summary>
        /// <param name="response">The response to write to</param>
        public void GenerateBadRequest(IHttpResponse response)
        {
            var page = _Server.NewPage();

            page.Title = "Malformed request";
            page.Value = "The server did not understand the request of your client.";

            response.Send(page);
        }

    }

}
