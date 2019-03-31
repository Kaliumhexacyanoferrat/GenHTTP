using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Core.General;

using Brotli;

namespace GenHTTP.Modules.Brotli.Compression
{

    public class BrotliAlgorithm : ICompressionAlgorithm
    {

        public string Name => "br";

        public Priority Priority => Priority.Medium;

        public Stream Compress(Stream content)
        {
            return new FilteredStream(content, (mem) => new BrotliStream(mem, CompressionMode.Compress));
        }

    }

}
