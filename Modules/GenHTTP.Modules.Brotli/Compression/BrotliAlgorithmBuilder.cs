using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Brotli.Compression
{

    public class BrotliAlgorithmBuilder : IBuilder<ICompressionAlgorithm>
    {

        public ICompressionAlgorithm Build()
        {
            return new BrotliAlgorithm();
        }

    }

}
