using System.IO;
using System.IO.Compression;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Core.General;

namespace GenHTTP.Examples.CoreApp.Utilities
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

}
