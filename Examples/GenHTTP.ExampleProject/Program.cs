using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using GenHTTP.Core;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Scriban;

namespace GenHTTP.ExampleProject
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var layout = Layout.Create()
                               .Template(ModScriban.Template(Data.FromResource("Template.html")))
                               .Add("res", Static.Resources("Resources"))
                               .Add("index", ModScriban.Page(Data.FromResource("Index.html")).Title("GenHTTP Webserver"), true);

            var server = Server.Create()
                               .Router(layout);

            using (var instance = server.Build())
            {
                Console.WriteLine("Press any key to stop ...");
                Console.ReadLine();
            }
        }
        
    }

}
