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
                               .Template(ModScriban.Template(Data.FromResource("Template.html").Build()).Build())
                               .Add("res", Static.Resources("Resources").Build())
                               .Add("index", ModScriban.Page(Data.FromResource("Index.html").Build()).Build())
                               .Index("index")
                               .Build();

            using (var server = new Server(layout, null, 8080))
            {
                Console.WriteLine("Press any key to stop ...");
                Console.ReadLine();
            }
        }
        
    }

}
