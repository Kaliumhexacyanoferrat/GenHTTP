using GenHTTP.Api.Content;
using GenHTTP.Modules.Core;

namespace GenHTTP.Examples.Examples.Infrastructure
{

    public static class InfrastructureExamples
    {

        public static IHandlerBuilder Create()
        {
            var proxy = ReverseProxy.Create()
                                    .Upstream("https://genhttp.org/");

            return Layout.Create()
                         .Add("exceptions", new ExceptionProviderBuilder())
                         .Add("proxy", proxy);
        }

    }

}
