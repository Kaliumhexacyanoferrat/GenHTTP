using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Core.General;

using BrotliSharpLib;

namespace GenHTTP.Modules.Core.Compression
{

    public class BrotliAlgorithm : ICompressionAlgorithm
    {

        public string Name => "br";

        public Priority Priority => Priority.Medium;

        public Stream Compress(Stream content)
        {
            return new FilteredStream(content, (mem) => {
                var stream = new BrotliStream(mem, CompressionMode.Compress, false);
                stream.SetQuality(7);

                return stream;
            });
        }

    }

}
