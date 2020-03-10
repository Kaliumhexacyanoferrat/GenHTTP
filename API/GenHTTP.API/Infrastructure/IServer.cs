using System;

using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Listens for incoming HTTP requests and dispatches them
    /// to the registered routers and content providers.
    /// </summary>
    public interface IServer : IDisposable
    {

        /// <summary>
        /// The version of the server software.
        /// </summary>
        /// <remarks>
        /// This property is for informational use only. Do not change
        /// your code depending on the version you are working with.
        /// </remarks>
        string Version { get; }

        /// <summary>
        /// If enabled, components may provide additional information
        /// allowing developers to further debug web applications.
        /// </summary>
        bool Development { get; }

        /// <summary>
        /// The endpoints the server is listening on.
        /// </summary>
        IEndPointCollection EndPoints { get; }

        /// <summary>
        /// An instance that will be called on certain events such as
        /// handled requests or errors that occur within the engine.
        /// </summary>
        IServerCompanion? Companion { get; }

        /// <summary>
        /// The extensions which are registered with the server and will
        /// be invoked for every handled request.
        /// </summary>
        IExtensionCollection Extensions { get; }

        /// <summary>
        /// The main router that will be used by the server to dispatch
        /// incoming HTTP requests.
        /// </summary>
        IRouter Router { get; }

    }

}
