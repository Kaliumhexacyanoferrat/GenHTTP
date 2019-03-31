using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

using Xunit;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Core.Tests.Compression;

namespace GenHTTP.Modules.Brotli.Tests
{

    public class ModBrotliTest
    {

        [Fact]
        public void TestCompression()
        {
            CompressionAlgorithmTests.Run(ModBrotli.Create().Build(), (compressed) => new BrotliStream(compressed, CompressionMode.Decompress));
        }

        [Fact]
        public void TestMetaData()
        {
            var algorithm = ModBrotli.Create().Build();

            Assert.Equal("br", algorithm.Name);
            Assert.Equal(Priority.Medium, algorithm.Priority);
        }

    }

}
