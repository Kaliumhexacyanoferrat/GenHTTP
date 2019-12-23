using System;

using GenHTTP.Core;

namespace GenHTTP.Examples.CoreApp
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var project = Project.Build();

            var server = Server.Create()
                               .Router(project)
                               .Console();

#if DEBUG
            server.Development();
#endif

            using (var instance = server.Build())
            {
                Console.WriteLine("Server is running ...");
                Console.WriteLine("Please press any key to shutdown ...");

                Console.ReadLine();
            }
        }

    }

}
