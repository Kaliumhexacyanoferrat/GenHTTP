
using System.Buffers;
using System.IO.Compression;

namespace GenHTTP.Modules.Compression.Algorithms;

internal sealed class BrotliCompressor : ICompressor
{
    private BrotliEncoder _encoder;

    internal BrotliCompressor(CompressionLevel level)
    {
        _encoder = new BrotliEncoder(MapQuality(level), MapWindow(level));
    }

    public OperationStatus Compress(ReadOnlySpan<byte> input, Span<byte> output, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
        => _encoder.Compress(input, output, out bytesConsumed, out bytesWritten, isFinalBlock);

    public void Dispose() => _encoder.Dispose();

    private static int MapQuality(CompressionLevel level) => level switch
    {
        CompressionLevel.Fastest => 0,
        CompressionLevel.Optimal => 4,
        CompressionLevel.SmallestSize => 11,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unsupported compression level.")
    };

    private static int MapWindow(CompressionLevel level) => level switch
    {
        CompressionLevel.Fastest => 16,
        CompressionLevel.Optimal => 20,
        CompressionLevel.SmallestSize => 22,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unsupported compression level.")
    };

}
