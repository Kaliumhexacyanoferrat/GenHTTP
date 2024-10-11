using System.IO.Compression;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class GzipAlgorithm : ICompressionAlgorithm
{

    public string Name => "gzip";

    public Priority Priority => Priority.Low;

    public IResponseContent Compress(IResponseContent content, CompressionLevel level)
    {
        return new CompressedResponseContent(content, target => new GZipStream(target, level, false));
    }
}
