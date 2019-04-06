using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Core;

using GenHTTP.Examples.CoreApp.Utilities;

namespace GenHTTP.Examples.CoreApp
{
    
    public static class Program
    {
        
        public static void Main(string[] args)
        {
            var project = Project.Build();
            
            var server = Server.Create()
                               .Router(project)
                               .Compression(new BrotliCompression())
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
