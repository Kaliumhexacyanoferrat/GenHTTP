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

namespace GenHTTP.ExampleProject
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var content = new Layout
            (
                routes: new Dictionary<string, IRouter>
                {
                    { "res", new EmbeddedResources("GenHTTP.ExampleProject.Resources") }
                },
                content: new Dictionary<string, IContentProvider>
                {
                    { "index", new TemplatedContent<TemplatedContentViewModel>(LoadTemplate("Pages.Index"), (rq, rs) => new TemplatedContentViewModel(rq, rs) { Title = "GenHTTP Webserver" }) }
                },
                index: "index",
                template: new TemplatedTemplate<TemplatedTemplateViewModel>(LoadTemplate("Templates.Template"), (rq, rs) => new TemplatedTemplateViewModel(rq, rs))
            );

            using (var server = EmbeddedServer.Run(content, null, 8080))
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
