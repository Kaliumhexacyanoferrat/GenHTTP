using System;
using System.Collections.Generic;
using System.Text;
using GenHTTP.Api.Routing;
using GenHTTP.Content.Basic;
using GenHTTP.Hosting.Embedded;

using Microsoft.Extensions.Logging;

namespace GenHTTP.ExampleProject
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var loggerFactory = new LoggerFactory().AddConsole();

            var content = new Layout
            (
                routes: new Dictionary<string, IRouter>
                {
                    { "res", new EmbeddedResources("GenHTTP.ExampleProject.Resources") }
                },
                index: "res/images/octocat.png"
            );

            using (var server = EmbeddedServer.Run(content, loggerFactory))
            {
                Console.WriteLine("Press any key to stop ...");
                Console.ReadLine();
            }
        }

    }

}
