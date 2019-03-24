using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Routing;

using GenHTTP.Content.Basic;
using GenHTTP.Content.Templating;
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
                content: new Dictionary<string, IContentProvider>
                {
                    { "index", new ScribanContent<ScribanContentViewModel>(LoadTemplate("Pages.Index"), (_) => new ScribanContentViewModel() { Title = "Index" }) }
                },
                index: "index",
                template: new ScribanTemplate(LoadTemplate("Templates.Template"))
            );

            using (var server = EmbeddedServer.Run(content, loggerFactory))
            {
                Console.WriteLine("Press any key to stop ...");
                Console.ReadLine();
            }
        }

        private static string LoadTemplate(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"GenHTTP.ExampleProject.{name}.html"))
            {
                return new StreamReader(stream).ReadToEnd();
            }
        }

    }

}
