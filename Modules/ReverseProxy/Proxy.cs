using GenHTTP.Modules.ReverseProxy.Provider;

namespace GenHTTP.Modules.ReverseProxy
{

    public static class Proxy
    {

        public static ReverseProxyBuilder Create()
        {
            return new ReverseProxyBuilder();
        }

    }

}
