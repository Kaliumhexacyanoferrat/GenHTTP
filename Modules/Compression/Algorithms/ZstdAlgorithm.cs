#if NET11_0_OR_GREATER

using System.IO.Compression;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Compression.Providers;

namespace GenHTTP.Modules.Compression.Algorithms;

public sealed class ZstdAlgorithm : ICompressionAlgorithm
{
    private static readonly AlgorithmName CachedName = new("zstd");

    public AlgorithmName Name => CachedName;

    public Priority Priority => Priority.High;

    public IResponseContent Compress(IResponseContent content, CompressionLevel level)
    {
        return new CompressedResponseContent(content, sink => new CompressingSink(sink, new ZstdCompressor(level)), Name);
    }

    public Stream Decompress(Stream content)
    {
        return new ZstandardStream(content, CompressionMode.Decompress, leaveOpen: true);
    }

}

#endif
