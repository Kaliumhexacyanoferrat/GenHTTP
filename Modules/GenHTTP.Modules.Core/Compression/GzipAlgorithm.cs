using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Compression
{

    public class GzipAlgorithm : ICompressionAlgorithm
    {

        public string Name => "gzip";

        public Priority Priority => Priority.Low;

        public Stream Compress(Stream content)
        {
            return new FilteredStream(content, (mem) => new GZipStream(mem, CompressionLevel.Fastest, false));
        }

    }

}
