using System.Buffers;

namespace GenHTTP.Modules.Compression.Algorithms;

internal interface ICompressor : IDisposable
{

    OperationStatus Compress(ReadOnlySpan<byte> input, Span<byte> output, out int bytesConsumed, out int bytesWritten, bool isFinalBlock);

}
