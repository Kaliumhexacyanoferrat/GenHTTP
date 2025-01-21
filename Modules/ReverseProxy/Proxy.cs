using GenHTTP.Modules.ReverseProxy.Provider;

namespace GenHTTP.Modules.ReverseProxy;

public static class Proxy
{

    /// <summary>
    /// Creates a handler that will route incoming requests to
    /// the specified upstream server.
    /// </summary>
    /// <returns>The newly created handler</returns>
    public static ReverseProxyBuilder Create() => new();

}
