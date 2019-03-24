using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// A project on the server's workspace.
    /// </summary>
    public interface IProject : IDisposable
    {

        /// <summary>
        /// Initialize this project.
        /// </summary>
        /// <param name="server">The server this project is running on</param>
        void Init(IServer server);

        /// <summary>
        /// The server associated with this project.
        /// </summary>
        IServer Server { get; }

        /// <summary>
        /// The unique name of the project.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The version of this project.
        /// </summary>
        Version Version { get; }
        
        /// <summary>
        /// The router providing the content of this project.
        /// </summary>
        IRouter Router { get; }

    }

}
