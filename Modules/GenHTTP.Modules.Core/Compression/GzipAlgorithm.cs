using System.IO.Compression;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Compression
{

    public class GzipAlgorithm : ICompressionAlgorithm
    {

        public string Name => "gzip";

        public Priority Priority => Priority.Low;
        
        public IResponseContent Compress(IResponseContent content)
        {
            return new CompressedContent(content, (target) => new GZipStream(target, CompressionLevel.Fastest, false));
        }

    }

}
