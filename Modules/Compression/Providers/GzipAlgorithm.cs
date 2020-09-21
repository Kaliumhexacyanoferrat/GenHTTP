using System.IO.Compression;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers
{

    public class GzipAlgorithm : ICompressionAlgorithm
    {

        public string Name => "gzip";

        public Priority Priority => Priority.Low;

        public IResponseContent Compress(IResponseContent content)
        {
            return new CompressedResponseContent(content, (target) => new GZipStream(target, CompressionLevel.Fastest, false));
        }

    }

}
