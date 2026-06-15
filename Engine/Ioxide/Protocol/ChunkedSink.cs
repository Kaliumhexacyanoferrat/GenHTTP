using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Response sink for unknown-length content: both the <see cref="Writer"/> and the
/// <see cref="Stream"/> route through a <see cref="ChunkedWriter"/>, so everything the content
/// writes is HTTP/1.1 chunk-framed. Call <see cref="Finish"/> after the content is written to
/// emit the terminating chunk.
/// </summary>
internal sealed class ChunkedSink : IResponseSink
{
    private readonly PipeWriter _writer;

    private readonly ChunkedWriter _chunked;

    private Stream? _stream;

    public ChunkedSink(PipeWriter writer)
    {
        _writer = writer;
        _chunked = new ChunkedWriter(writer);
    }

    public IBufferWriter<byte> Writer => _chunked;

    public Stream Stream => _stream ??= new PipeWriterStream(_chunked, _writer);

    public void Finish() => _chunked.Finish();
}
