using GenHTTP.Modules.Core.Proxy;

namespace GenHTTP.Modules.Core
{

    public static class ReverseProxy
    {

        public static ReverseProxyBuilder Create()
        {
            return new ReverseProxyBuilder();
        }

    }

}
