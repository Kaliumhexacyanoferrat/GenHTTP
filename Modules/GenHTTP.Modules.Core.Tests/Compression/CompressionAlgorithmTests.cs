using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Xunit;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Core.Tests.Compression
{

    public static class CompressionAlgorithmTests
    {

        public static void Run(ICompressionAlgorithm algorithm, Func<Stream, Stream> decompressionHandler)
        {
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes("Hello Algorithm!")))
            {
                using (var decompression = decompressionHandler(algorithm.Compress(input)))
                {
                    using (var reader = new StreamReader(decompression))
                    {
                        Assert.Equal("Hello Algorithm!", reader.ReadToEnd());
                    }
                }
            }
        }

    }

}
