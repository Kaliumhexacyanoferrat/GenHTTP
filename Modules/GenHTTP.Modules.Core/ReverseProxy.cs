using GenHTTP.Modules.Core.Proxy;

namespace GenHTTP.Modules.Core
{

    public static class ReverseProxy
    {

        public static ReverseProxyProviderBuilder Create()
        {
            return new ReverseProxyProviderBuilder();
        }

    }

}
