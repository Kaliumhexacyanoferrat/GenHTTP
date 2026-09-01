using System.Buffers;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Compression.Algorithms;

internal sealed class CompressingSink : IResponseSink, IAsyncDisposable, IDisposable
{
    private const int InputBufferSize = BufferSize.Write;
    private const int OutputBufferSize = BufferSize.Write;

    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

    private readonly IResponseSink _inner;
    private readonly ICompressor _compressor;
    private readonly IBufferWriter<byte> _writer;

    private readonly byte[] _inputBuffer;

    private bool _disposed;

    private sealed class EncoderBufferWriter : IBufferWriter<byte>
    {
        private readonly CompressingSink _sink;

        internal EncoderBufferWriter(CompressingSink sink)
        {
            _sink = sink;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if ((uint)sizeHint > (uint)_sink._inputBuffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeHint));
            }

            return _sink._inputBuffer;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if ((uint)sizeHint > (uint)_sink._inputBuffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeHint));
            }

            return _sink._inputBuffer;
        }

        public void Advance(int count)
        {
            if ((uint)count > (uint)_sink._inputBuffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            _sink.Compress(
                _sink._inputBuffer.AsSpan(0, count),
                isFinalBlock: false);
        }
    }

    internal CompressingSink(IResponseSink inner, ICompressor compressor)
    {
        _inner = inner;
        _compressor = compressor;

        _inputBuffer = Pool.Rent(InputBufferSize);
        _writer = new EncoderBufferWriter(this);
    }

    public IBufferWriter<byte> Writer => _writer;

    public Stream Stream => _stream ??= new WriterStreamAdapter(_writer);

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
