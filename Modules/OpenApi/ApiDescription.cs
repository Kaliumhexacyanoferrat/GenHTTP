using GenHTTP.Modules.OpenApi.Discovery;
using GenHTTP.Modules.OpenApi.Handler;

namespace GenHTTP.Modules.OpenApi;

public static class ApiDescription
{

    public static OpenApiConcernBuilder Create() => With(ApiDiscovery.Default());

    public static OpenApiConcernBuilder With(ApiDiscoveryRegistryBuilder discovery) => new(discovery.Build());
}
