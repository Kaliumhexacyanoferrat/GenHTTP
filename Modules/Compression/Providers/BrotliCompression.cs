using System.IO.Compression;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class BrotliCompression : ICompressionAlgorithm
{

    public string Name => "br";

    public Priority Priority => Priority.Medium;

    public IResponseContent Compress(IResponseContent content, CompressionLevel level)
    {
        return new CompressedResponseContent(content, target => new BrotliStream(target, level, false));
    }
}
