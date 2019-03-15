using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Http;

namespace GenHTTP.Api.Project
{

    /// <summary>
    /// A project on the server's workspace.
    /// </summary>
    public interface IProject
    {

        /// <summary>
        /// Initialize this project. Allows the project to load data before the
        /// first request arrives.
        /// </summary>
        /// <param name="server">The server this project is running on</param>
        /// <param name="localFolder">The project folder this project lies in</param>
        /// <param name="log">An opened log file handler. Can be used to log information to a project-specific log file.</param>
        void Init(IServer server, string localFolder, ILog log);

        /// <summary>
        /// The version of this project.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// The name of the folder the project's assembly lies in.
        /// </summary>
        string LocalFolder { get; }

        /// <summary>
        /// The unique name of the project.
        /// </summary>
        /// <remarks>
        /// The <see cref="ClientHandler" /> uses this field to determine the selected project.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// This method will get called directly before the application exits.
        /// </summary>
        void Dump();

        /// <summary>
        /// Handles a <see cref="IHttpRequest" />. Called by the responsible <see cref="ClientHandler" />.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <param name="response">The response to write to</param>
        /// <remarks>
        /// This method replaces the interface IProjectHandler which was used in previous versions of the GenHTTP library (before v1.11).
        /// </remarks>
        void HandleRequest(IHttpRequest request, IHttpResponse response);

    }

}
