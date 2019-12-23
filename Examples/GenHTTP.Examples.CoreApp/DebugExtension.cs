using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Examples.CoreApp
{

    public static class DebugExtension
    {

        public static IServerHost Debug(this IServerHost host)
        {
#if DEBUG
            return host.Development();
#else
            return host;
#endif
        }

    }

}
