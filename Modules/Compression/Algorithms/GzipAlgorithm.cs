using System.IO.Compression;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Compression.Providers;

namespace GenHTTP.Modules.Compression.Algorithms;

public sealed class GzipAlgorithm : ICompressionAlgorithm
{

    public string Name => "gzip";

    public Priority Priority => Priority.Low;

    public IResponseContent Compress(IResponseContent content, CompressionLevel level)
    {
        return new CompressedResponseContent(content, target => new GZipStream(target, level, false));
    }

    public Stream Decompress(Stream content)
    {
        return new GZipStream(content, CompressionMode.Decompress, leaveOpen: true);
    }

}
