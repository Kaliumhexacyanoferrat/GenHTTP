using GenHTTP.Modules.OpenApi.Discovery;

namespace GenHTTP.Modules.OpenApi;

/// <summary>
/// Provides capabilities used by the OpenAPI generation feature to analyze
/// the functionality provided by your API.
/// </summary>
public static class ApiDiscovery
{

    /// <summary>
    /// Creates an empty registry.
    /// </summary>
    /// <returns>The newly created, empty registry</returns>
    public static ApiDiscoveryRegistryBuilder Empty() => new();

    /// <summary>
    /// Creates a registry that supports layouts, concerns, functional handlers,
    /// controllers and webservices for automatic content discovery.
    /// </summary>
    /// <returns>The default registry to use as a basis</returns>
    public static ApiDiscoveryRegistryBuilder Default() => Empty().Add<ConcernExplorer>()
                                                                  .Add<LayoutExplorer>()
                                                                  .Add<ServiceExplorer>()
                                                                  .Add<MethodCollectionExplorer>()
                                                                  .Add<MethodHandlerExplorer>();
}
