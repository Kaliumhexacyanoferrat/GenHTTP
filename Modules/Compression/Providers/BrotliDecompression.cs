using System.IO.Compression;

namespace GenHTTP.Modules.Compression.Providers;

/// <summary>
/// Decompression algorithm for brotli-encoded request content.
/// </summary>
public sealed class BrotliDecompression : IDecompressionAlgorithm
{
    public string Name => "br";

    public Stream Decompress(Stream content)
    {
        return new BrotliStream(content, CompressionMode.Decompress, leaveOpen: false);
    }
}
