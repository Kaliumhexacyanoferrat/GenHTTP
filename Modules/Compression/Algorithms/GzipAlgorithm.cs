using System.IO.Compression;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Compression.Providers;

namespace GenHTTP.Modules.Compression.Algorithms;

public sealed class GzipAlgorithm : ICompressionAlgorithm
{
    private static readonly AlgorithmName CachedName = new("gzip");

    public AlgorithmName Name => CachedName;

    public Priority Priority => Priority.Low;

    public IResponseContent Compress(IResponseContent content, CompressionLevel level)
    {
        return new CompressedResponseContent(content, sink => new StreamCompressingSink(sink, target => new GZipStream(target, level, leaveOpen: false)), Name);
    }

    public Stream Decompress(Stream content)
    {
        return new GZipStream(content, CompressionMode.Decompress, leaveOpen: true);
    }

}
