using GenHTTP.Modules.OpenApi.Discovery;

namespace GenHTTP.Modules.OpenApi;

public static class ApiDiscovery
{

    public static ApiDiscoveryRegistryBuilder Empty() => new();

    public static ApiDiscoveryRegistryBuilder Default() => Empty().Add<ConcernExplorer>()
                                                                  .Add<LayoutExplorer>()
                                                                  .Add<InlineExplorer>()
                                                                  .Add<MethodCollectionExplorer>()
                                                                  .Add<MethodHandlerExplorer>();

}
