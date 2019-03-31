using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;

using Xunit;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Core.Compression;

namespace GenHTTP.Modules.Core.Tests.Compression
{

    public class GzipAlgorithmTests
    {
        
        [Fact]
        public void TestCompression()
        {
            CompressionAlgorithmTests.Run(new GzipAlgorithm(), (compressed) => new GZipStream(compressed, CompressionMode.Decompress));
        }

        [Fact]
        public void TestMetaData()
        {
            var algorithm = new GzipAlgorithm();

            Assert.Equal("gzip", algorithm.Name);
            Assert.Equal(Priority.Low, algorithm.Priority);
        }
        
    }

}
