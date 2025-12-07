using System.IO.Compression;

namespace GenHTTP.Modules.Compression.Providers;

/// <summary>
/// Decompression algorithm for deflate-encoded request content.
/// </summary>
public sealed class DeflateDecompression : IDecompressionAlgorithm
{
    public string Name => "deflate";

    public Stream Decompress(Stream content)
    {
        return new DeflateStream(content, CompressionMode.Decompress, leaveOpen: false);
    }
}
