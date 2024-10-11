using System.IO.Compression;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using ZstdSharp;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class ZstdCompression : ICompressionAlgorithm
{

    public string Name => "zstd";

    public Priority Priority => Priority.High;

    public IResponseContent Compress(IResponseContent content, CompressionLevel level)
    {
        return new CompressedResponseContent(content, (target) => new CompressionStream(target, level: MapLevel(level), leaveOpen: false));
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

}
