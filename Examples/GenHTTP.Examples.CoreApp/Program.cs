using System;

using GenHTTP.Core;
using GenHTTP.Modules.Brotli;

namespace GenHTTP.Examples.CoreApp
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var project = Project.Build();

            var server = Server.Create()
                               .Router(project)
                               .Compression(ModBrotli.Create())
                               .Console();

            using (var instance = server.Build())
            {
                Console.WriteLine("Server is running on port 8080.");
                Console.WriteLine("Please press any key to shutdown ...");

                Console.ReadLine();
            }
        }

    }

}
