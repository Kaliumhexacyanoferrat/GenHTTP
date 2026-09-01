#if NET11_0_OR_GREATER

using System.Buffers;
using System.IO.Compression;

namespace GenHTTP.Modules.Compression.Algorithms;

internal sealed class ZstdCompressor : ICompressor
{
    private ZstandardEncoder _encoder;

    internal ZstdCompressor(CompressionLevel level)
    {
        _encoder = new ZstandardEncoder(MapQuality(level));
    }

    public OperationStatus Compress(ReadOnlySpan<byte> input, Span<byte> output, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
        => _encoder.Compress(input, output, out bytesConsumed, out bytesWritten, isFinalBlock);

    public void Dispose() => _encoder.Dispose();

    private static int MapQuality(CompressionLevel level) => level switch
    {
        CompressionLevel.Fastest => 1,
        CompressionLevel.Optimal => 3,
        CompressionLevel.SmallestSize => 19,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unsupported compression level.")
    };

}

#endif
