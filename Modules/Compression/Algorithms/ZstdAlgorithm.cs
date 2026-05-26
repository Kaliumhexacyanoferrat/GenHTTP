using System.IO.Compression;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Compression.Providers;
using ZstdSharp;

namespace GenHTTP.Modules.Compression.Algorithms;

public sealed class ZstdAlgorithm : ICompressionAlgorithm
{
    private static readonly AlgorithmName CachedName = new("zstd");

    public AlgorithmName Name => CachedName;

    public Priority Priority => Priority.High;

    public IResponseContent Compress(IResponseContent content, CompressionLevel level)
    {
        return new CompressedResponseContent(content, target => new CompressionStream(target, MapLevel(level), leaveOpen: false), Name);
    }

    private static int MapLevel(CompressionLevel level)
    {
        return level switch
        {
            CompressionLevel.Fastest => 1,
            CompressionLevel.SmallestSize => 22,
            CompressionLevel.Optimal => 3,
            _ => throw new InvalidOperationException($"Unable to map compression level {level}")
        };
    }

    public Stream Decompress(Stream content)
    {
        return new DecompressionStream(content, leaveOpen: true);
    }

}
