using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Brotli.Compression;

namespace GenHTTP.Modules.Brotli
{

    public static class ModBrotli
    {

        public static IBuilder<ICompressionAlgorithm> Create()
        {
            return new BrotliAlgorithmBuilder();
        }

    }

}
