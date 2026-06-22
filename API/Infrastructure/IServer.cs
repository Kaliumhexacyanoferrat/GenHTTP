using GenHTTP.Api.Content;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// Listens for incoming HTTP requests and dispatches them
/// to the registered routers and content providers.
/// </summary>
public interface IServer : IAsyncDisposable
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
    /// Specifies, whether the server still serves requests or
    /// whether it is currently shut down.
    /// </summary>
    bool Running { get; }

    /// <summary>
    /// If enabled, components may provide additional information
    /// allowing developers to further debug web applications.
    /// </summary>
    bool Development { get; }

    /// <summary>
    /// Property bag to store values during the lifetime of the server.
    /// </summary>
    IPropertyBag Properties { get; }

    /// <summary>
    /// The logging infrastructure of the application, used by the server itself
    /// as well as by the handlers, concerns and other components running within it
    /// to emit diagnostic information.
    /// </summary>
    /// <remarks>
    /// Logs to the console by default. Use <see cref="IServerHost.Logging(ILoggerFactory, bool)" />
    /// to configure a different factory or to disable logging entirely.
    /// </remarks>
    ILoggerFactory Logging { get; }
    
    /// <summary>
    /// The endpoints the server is listening on.
    /// </summary>
    IEndPointCollection EndPoints { get; }

    /// <summary>
    /// The main router that will be used by the server to dispatch
    /// incoming HTTP requests.
    /// </summary>
    IHandler Handler { get; }

    /// <summary>
    /// Starts to listen for requests, shut down by disposing
    /// this instance.
    /// </summary>
    ValueTask StartAsync();

}
