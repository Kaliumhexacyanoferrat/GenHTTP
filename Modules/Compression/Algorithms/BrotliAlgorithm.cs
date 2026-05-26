using System.IO.Compression;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Compression.Providers;

namespace GenHTTP.Modules.Compression.Algorithms;

public sealed class BrotliAlgorithm : ICompressionAlgorithm
{
    private static readonly AlgorithmName CachedName = new("br");

    public AlgorithmName Name => CachedName;

    public Priority Priority => Priority.Medium;

    public IResponseContent Compress(IResponseContent content, CompressionLevel level)
    {
        return new CompressedResponseContent(content, target => new BrotliStream(target, level, false), Name);
    }

    public Stream Decompress(Stream content)
    {
        return new BrotliStream(content, CompressionMode.Decompress, leaveOpen: true);
    }

}
