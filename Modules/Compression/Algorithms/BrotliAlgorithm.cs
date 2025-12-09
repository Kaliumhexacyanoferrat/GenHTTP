using System.IO.Compression;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Compression.Providers;

namespace GenHTTP.Modules.Compression.Algorithms;

public sealed class BrotliAlgorithm : ICompressionAlgorithm
{

    public string Name => "br";

    public Priority Priority => Priority.Medium;

    public IResponseContent Compress(IResponseContent content, CompressionLevel level)
    {
        return new CompressedResponseContent(content, target => new BrotliStream(target, level, false));
    }

    public Stream Decompress(Stream content)
    {
        return new BrotliStream(content, CompressionMode.Decompress, leaveOpen: true);
    }

}
