using System.Buffers;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Compression.Algorithms;

internal sealed class CompressingSink<TCompressor> : IResponseSink, IBufferWriter<byte>, IAsyncDisposable, IDisposable where TCompressor : struct, ICompressor
{
    private const int InputBufferSize = BufferSize.Write;
    private const int OutputBufferSize = BufferSize.Write;

    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

    private readonly IResponseSink _inner;

    private TCompressor _compressor;

    private byte[] _inputBuffer;

    private bool _disposed;

    internal CompressingSink(IResponseSink inner, TCompressor compressor)
    {
        _inner = inner;
        _compressor = compressor;

        _inputBuffer = Pool.Rent(InputBufferSize);
    }

    public IBufferWriter<byte> Writer => this;

    public Span<byte> GetSpan(int sizeHint = 0) => EnsureInputBuffer(sizeHint);

    public Memory<byte> GetMemory(int sizeHint = 0) => EnsureInputBuffer(sizeHint);

    public void Advance(int count)
    {
        if ((uint)count > (uint)_inputBuffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        Compress(_inputBuffer.AsSpan(0, count), isFinalBlock: false);
    }

    /// <summary>
    /// Returns the input buffer, replacing it with a larger one if the caller
    /// requests more space than it offers (e.g. a serializer writing a large value).
    /// </summary>
    /// <remarks>
    /// Everything written so far has already been handed to the compressor
    /// on <see cref="IBufferWriter{T}.Advance"/>, so there is nothing to preserve.
    /// </remarks>
    private byte[] EnsureInputBuffer(int sizeHint)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);

        if (sizeHint > _inputBuffer.Length)
        {
            var larger = Pool.Rent(sizeHint);

            Pool.Return(_inputBuffer);

            _inputBuffer = larger;
        }

        return _inputBuffer;
    }

    public Stream Stream => _stream ??= new WriterStreamAdapter(this);

    private WriterStreamAdapter? _stream;

    private void Compress(ReadOnlySpan<byte> input, bool isFinalBlock)
    {
        var writer = _inner.Writer;

        while (true)
        {
            var output = writer.GetSpan(OutputBufferSize);

            var status = _compressor.Compress(
                input,
                output,
                out var consumed,
                out var written,
                isFinalBlock);

            if (written != 0)
            {
                writer.Advance(written);
            }

            input = input[consumed..];

            switch (status)
            {
                case OperationStatus.Done:
                    return;

                case OperationStatus.NeedMoreData:
                    if (isFinalBlock)
                    {
                        throw new InvalidOperationException(
                            "The compression encoder requested more data while finalizing.");
                    }

                    return;

                case OperationStatus.DestinationTooSmall:
                    // The compressor needs another output buffer.
                    continue;

                case OperationStatus.InvalidData:
                    throw new InvalidDataException(
                        "The compression encoder rejected the input data.");

                default:
                    throw new InvalidOperationException(
                        $"Unexpected compression encoder status: {status}.");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            Compress(ReadOnlySpan<byte>.Empty, isFinalBlock: true);
        }
        finally
        {
            _compressor.Dispose();
            Pool.Return(_inputBuffer);
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
    
}
