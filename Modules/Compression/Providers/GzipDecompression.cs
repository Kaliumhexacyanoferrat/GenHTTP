using System.IO.Compression;

namespace GenHTTP.Modules.Compression.Providers;

/// <summary>
/// Decompression algorithm for gzip-encoded request content.
/// </summary>
public sealed class GzipDecompression : IDecompressionAlgorithm
{
    public string Name => "gzip";

    public Stream Decompress(Stream content)
    {
        return new GZipStream(content, CompressionMode.Decompress, leaveOpen: false);
    }
}
