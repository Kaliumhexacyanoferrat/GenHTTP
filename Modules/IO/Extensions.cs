using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO
{

    public static class Extensions
    {

        public static IServerHost RangeSupport(this IServerHost host)
        {
            host.Add(IO.RangeSupport.Create());
            return host;
        }

    }

}
