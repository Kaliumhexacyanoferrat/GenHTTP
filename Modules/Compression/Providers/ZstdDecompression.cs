using ZstdSharp;

namespace GenHTTP.Modules.Compression.Providers;

/// <summary>
/// Decompression algorithm for zstandard-encoded request content.
/// </summary>
public sealed class ZstdDecompression : IDecompressionAlgorithm
{
    public string Name => "zstd";

    public Stream Decompress(Stream content)
    {
        return new DecompressionStream(content, leaveOpen: false);
    }
}
