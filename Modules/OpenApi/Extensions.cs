using GenHTTP.Api.Content;
using GenHTTP.Modules.OpenApi.Discovery;

namespace GenHTTP.Modules.OpenApi;

public static class Extensions
{

    /// <summary>
    /// Adds a pre-configured concern to the given builder.
    /// </summary>
    /// <remarks>
    /// The generated concern will crawl through the inner handler chain and analyze the following
    /// types of content: Layouts, Concerns, Functional Handlers, Webservices, Controllers.
    /// </remarks>
    public static T AddOpenApi<T>(this T builder) where T : IHandlerBuilder<T>
        => builder.AddOpenApi(ApiDiscovery.Default());

    /// <summary>
    /// Creates a concern that will use the given discovery configuration to search for API endpoints
    /// to be added to the generated OpenAPI specification.
    /// </summary>
    /// <param name="registry">The explorer registry to be used to analyze the handler chain</param>
    public static T AddOpenApi<T>(this T builder, ApiDiscoveryRegistryBuilder registry) where T : IHandlerBuilder<T>
    {
        builder.Add(ApiDescription.With(registry));
        return builder;
    }

}
