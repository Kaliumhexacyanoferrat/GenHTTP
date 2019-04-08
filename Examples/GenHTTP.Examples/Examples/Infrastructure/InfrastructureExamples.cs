using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Modules.Core;

namespace GenHTTP.Examples.Examples.Infrastructure
{

    public static class InfrastructureExamples
    {

        public static IRouterBuilder Create()
        {
            var proxy = ReverseProxy.Create()
                                    .Upstream("https://genes.pics/genhttp/website/");

            return Layout.Create()
                         .Add("exceptions", new ExceptionProvider())
                         .Add("proxy", proxy);
        }

    }

}
