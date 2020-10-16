using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.ClientCaching
{

    public static class Extensions
    {

        public static IServerHost ClientCaching(this IServerHost host)
        {
            host.Add(ClientCache.Validation());
            return host;
        }

    }

}
