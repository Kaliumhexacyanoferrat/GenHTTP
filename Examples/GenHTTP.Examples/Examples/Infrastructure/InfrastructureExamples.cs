using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core;

namespace GenHTTP.Examples.Examples.Infrastructure
{

    public static class InfrastructureExamples
    {

        public static IRouterBuilder Create()
        {
            var proxy = ReverseProxy.Create()
                                    .Upstream("https://genhttp.org/");

            return Layout.Create()
                         .Add("exceptions", new ExceptionProvider())
                         .Add("proxy", proxy);
        }

    }

}
