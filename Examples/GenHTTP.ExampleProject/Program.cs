using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Hosting.Embedded;

using Microsoft.Extensions.Logging;

namespace GenHTTP.ExampleProject
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var loggerFactory = new LoggerFactory().AddConsole();

            using (var server = EmbeddedServer.Run(new ExampleRouter(), loggerFactory))
            {
                Console.WriteLine("Press any key to stop ...");
                Console.ReadLine();
            }
        }

    }

}
