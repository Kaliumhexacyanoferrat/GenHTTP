using GenHTTP.Modules.OpenApi.Discovery;
using GenHTTP.Modules.OpenApi.Handler;

namespace GenHTTP.Modules.OpenApi;

/// <summary>
/// Provides a concern that will analyze the inner handler and
/// automatically generate an OpenAPI specification and
/// provide it to the clients when requested.
/// </summary>
public static class ApiDescription
{

    /// <summary>
    /// Creates a pre-configured concern to be added to a layout or any other handler.
    /// </summary>
    /// <returns>The pre-configured concern that will generate an OpenAPI specification on request</returns>
    /// <remarks>
    /// The generated concern will crawl through the inner handler chain and analyze the following
    /// types of content: Layouts, Concerns, Functional Handlers, Webservices, Controllers.
    /// If you use other handlers or specific concerns to provide your API, you will need to implement
    /// <see cref="IApiExplorer" /> instances and pass them to the <see cref="With" /> method.
    /// </remarks>
    public static OpenApiConcernBuilder Create() => With(ApiDiscovery.Default());

    /// <summary>
    /// Creates a concern that will use the given discovery configuration to search for API endpoints
    /// to be added to the generated OpenAPI specification.
    /// </summary>
    /// <param name="discovery">The explorer registry to be used to analyze the handler chain</param>
    /// <returns>The newly generated concern</returns>
    public static OpenApiConcernBuilder With(ApiDiscoveryRegistryBuilder discovery) => new(discovery.Build());

}
