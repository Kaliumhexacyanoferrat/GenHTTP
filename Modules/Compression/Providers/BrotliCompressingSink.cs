using System.Buffers;
using System.IO.Compression;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Compression.Providers;

internal sealed class BrotliCompressingSink : IResponseSink, IAsyncDisposable, IDisposable
{
    private readonly IResponseSink _inner;
    private BrotliEncoder _encoder;
    private readonly byte[] _inputBuffer;

    private EncoderBufferWriter? _bufferWriter;
    private WriterStreamAdapter? _stream;

    #region Supporting data structures

    private sealed class EncoderBufferWriter : IBufferWriter<byte>
    {
        private readonly BrotliCompressingSink _sink;

        internal EncoderBufferWriter(BrotliCompressingSink sink) => _sink = sink;

        public Memory<byte> GetMemory(int sizeHint = 0) => _sink._inputBuffer;

        public Span<byte> GetSpan(int sizeHint = 0) => _sink._inputBuffer;

        public void Advance(int count) => _sink.CompressChunk(_sink._inputBuffer.AsSpan(0, count), isFinalBlock: false);
    }

    #endregion

    #region Get-/Setters

    public IBufferWriter<byte> Writer => _bufferWriter ??= new EncoderBufferWriter(this);

    public Stream Stream => _stream ??= new WriterStreamAdapter(Writer);

    #endregion

    #region Initialization

    internal BrotliCompressingSink(IResponseSink inner, CompressionLevel level)
    {
        _inner = inner;
        _inputBuffer = ArrayPool<byte>.Shared.Rent(BufferSize.Write);
        _encoder = new BrotliEncoder(MapQuality(level), MapWindow(level));
    }

    #endregion

    #region Functionality

    private void CompressChunk(ReadOnlySpan<byte> input, bool isFinalBlock)
    {
        OperationStatus status;

        do
        {
            var writer = _inner.Writer;

            var output = writer.GetSpan(2048);

            status = _encoder.Compress(input, output, out int consumed, out int written, isFinalBlock);

            if (written > 0)
            {
                writer.Advance(written);
            }

            input = input.Slice(consumed);
        }
        while (status == OperationStatus.DestinationTooSmall || (isFinalBlock && status != OperationStatus.Done && input.Length > 0));
    }

    private static int MapQuality(CompressionLevel level) => level switch
    {
        CompressionLevel.Fastest => 0,
        CompressionLevel.Optimal => 4,
        CompressionLevel.SmallestSize => 11,
        _ => throw new InvalidOperationException($"Unsupported compression level: {level}")
    };

    private static int MapWindow(CompressionLevel level) => level switch
    {
        CompressionLevel.Fastest => 16,       // 64 KB — adequate for typical HTTP responses
        CompressionLevel.Optimal => 20,       // 1 MB
        CompressionLevel.SmallestSize => 22,  // 4 MB — full window for best ratio
        _ => throw new InvalidOperationException($"Unsupported compression level: {level}")
    };

    #endregion

    #region IDisposable Support

    public void Dispose()
    {
        CompressChunk(ReadOnlySpan<byte>.Empty, isFinalBlock: true);
        _encoder.Dispose();
        ArrayPool<byte>.Shared.Return(_inputBuffer);
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    #endregion

}
