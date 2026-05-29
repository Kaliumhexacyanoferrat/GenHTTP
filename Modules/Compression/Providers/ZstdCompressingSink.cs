using System.Buffers;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;
using ZstdSharp;

namespace GenHTTP.Modules.Compression.Providers;

internal sealed class ZstdCompressingSink : IResponseSink, IAsyncDisposable, IDisposable
{
    private const int InputBufferSize = 65536;

    private readonly IResponseSink _inner;
    private readonly Compressor _compressor;
    private readonly byte[] _inputBuffer;

    private EncoderBufferWriter? _bufferWriter;
    private WriterStreamAdapter? _stream;
    
    #region Supporting data structures

    private sealed class EncoderBufferWriter : IBufferWriter<byte>
    {
        private readonly ZstdCompressingSink _sink;

        internal EncoderBufferWriter(ZstdCompressingSink sink) => _sink = sink;

        public Memory<byte> GetMemory(int sizeHint = 0) => _sink._inputBuffer;

        public Span<byte> GetSpan(int sizeHint = 0) => _sink._inputBuffer;

        public void Advance(int count) => _sink.CompressChunk(_sink._inputBuffer.AsSpan(0, count));
    }

    #endregion
    
    #region Get-/Setters

    public IBufferWriter<byte> Writer => _bufferWriter ??= new EncoderBufferWriter(this);

    public Stream Stream => _stream ??= new WriterStreamAdapter(Writer);

    #endregion

    #region Initialization

    internal ZstdCompressingSink(IResponseSink inner, int level)
    {
        _inner = inner;
        _inputBuffer = ArrayPool<byte>.Shared.Rent(InputBufferSize);
        _compressor = new Compressor(level);
    }

    #endregion

    #region Functionality

    internal void CompressChunk(ReadOnlySpan<byte> input)
    {
        OperationStatus status;

        do
        {
            var output = _inner.Writer.GetSpan(256);
            status = _compressor.WrapStream(input, output, out int consumed, out int written, isFinalBlock: false);
            _inner.Writer.Advance(written);
            input = input.Slice(consumed);
        }
        while (status == OperationStatus.DestinationTooSmall);
    }

    private void FinalFlush()
    {
        OperationStatus status;

        do
        {
            var output = _inner.Writer.GetSpan(256);
            status = _compressor.FlushStream(output, out int written, isFinalBlock: true);
            _inner.Writer.Advance(written);
        }
        while (status != OperationStatus.Done);
    }

    #endregion

    #region IDisposable Support

    public void Dispose()
    {
        FinalFlush();
        _compressor.Dispose();
        ArrayPool<byte>.Shared.Return(_inputBuffer);
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    #endregion

}
