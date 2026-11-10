using System.IO.Compression;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Compression.Providers;

namespace GenHTTP.Modules.Compression.Algorithms;

public sealed class BrotliAlgorithm : ICompressionAlgorithm
{
    private static readonly AlgorithmName CachedName = new("br");

    private static readonly SinkFactories Factories = new(level => sink => new CompressingSink<BrotliCompressor>(sink, new BrotliCompressor(level)));

    public AlgorithmName Name => CachedName;

    public string FileExtension => "br";

    public Priority Priority => Priority.Medium;

    public IResponseContent Compress(IResponseContent content, CompressionLevel level)
    {
        return new CompressedResponseContent(content, Factories.Get(level), Name);
    }

    public Stream Decompress(Stream content)
    {
        return new BrotliStream(content, CompressionMode.Decompress, leaveOpen: true);
    }

}
