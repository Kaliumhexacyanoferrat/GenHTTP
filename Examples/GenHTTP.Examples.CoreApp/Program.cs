using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Core;
using GenHTTP.Modules.Core.General;

namespace GenHTTP.Examples.CoreApp
{

    public class BrotliCompression : ICompressionAlgorithm
    {

        public string Name => "br";

        public Priority Priority => Priority.High;

        public Stream Compress(Stream content)
        {
            return new FilteredStream(content, (mem) => new BrotliStream(mem, CompressionLevel.Fastest, false));
        }

    }

    public static class Program
    {
        
        public static void Main(string[] args)
        {
            var project = Project.Build();
            
            var server = Server.Create()
                               .Router(project)
                               .Compression(new BrotliCompression())
                               .Console();

            using (var instance = server.Build())
            {
                Console.WriteLine("Server is running ...");
                Console.WriteLine("Please press any key to shutdown ...");

                Console.ReadLine();
            }
        }

    }

}
