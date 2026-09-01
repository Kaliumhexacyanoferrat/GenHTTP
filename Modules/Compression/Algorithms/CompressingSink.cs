using System.Buffers;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Compression.Algorithms;

internal sealed class CompressingSink : IResponseSink, IAsyncDisposable, IDisposable
{
    private const int OutputBufferSize = 2048;

    private readonly IResponseSink _inner;
    private readonly ICompressor _compressor;

    private readonly byte[] _inputBuffer;

    private EncoderBufferWriter? _bufferWriter;
    private WriterStreamAdapter? _stream;

    private bool _disposed;

    #region Supporting data structures

    private sealed class EncoderBufferWriter : IBufferWriter<byte>
    {
        private readonly CompressingSink _sink;

        internal EncoderBufferWriter(CompressingSink sink) { _sink = sink; }

        public Memory<byte> GetMemory(int sizeHint = 0) => _sink._inputBuffer;

        public Span<byte> GetSpan(int sizeHint = 0) => _sink._inputBuffer;

        public void Advance(int count)
        {
            if ((uint)count > (uint)_sink._inputBuffer.Length) { throw new ArgumentOutOfRangeException(nameof(count)); }
            _sink.CompressChunk(_sink._inputBuffer.AsSpan(0, count), isFinalBlock: false);
        }
    }

    #endregion

    #region Get-/Setters

    public IBufferWriter<byte> Writer => _bufferWriter ??= new EncoderBufferWriter(this);

    public Stream Stream => _stream ??= new WriterStreamAdapter(Writer);

    #endregion

    #region Initialization

    internal CompressingSink(IResponseSink inner, ICompressor compressor)
    {
        _inner = inner;
        _compressor = compressor;
        _inputBuffer = ArrayPool<byte>.Shared.Rent(BufferSize.Write);
    }

    #endregion

    #region Functionality

    private void CompressChunk(ReadOnlySpan<byte> input, bool isFinalBlock)
    {
        do
        {
            var writer = _inner.Writer;
            var output = writer.GetSpan(OutputBufferSize);
            var status = _compressor.Compress(input, output, out var consumed, out var written, isFinalBlock);
            if (written > 0) { writer.Advance(written); }
            input = input[consumed..];
            if (status == OperationStatus.InvalidData) { throw new InvalidDataException("The compression encoder rejected the input data."); }
            if (status == OperationStatus.DestinationTooSmall) { continue; }
            if (status == OperationStatus.NeedMoreData)
            {
                if (isFinalBlock) { throw new InvalidOperationException("The compression encoder requested more data while finalizing."); }
                break;
            }
            if (status == OperationStatus.Done) { break; }
            throw new InvalidOperationException($"Unexpected compression encoder status: {status}.");
        }
        while (true);
    }

    #endregion

    #region IDisposable Support

    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;
        try { CompressChunk(ReadOnlySpan<byte>.Empty, isFinalBlock: true); }
        finally
        {
            _compressor.Dispose();
            ArrayPool<byte>.Shared.Return(_inputBuffer);
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    #endregion

}
