using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Response sink for unknown-length content: both channels route through a
/// <see cref="ChunkedWriter"/>, so everything the content writes is chunk-framed.
/// <see cref="Finish"/> emits the terminating chunk.
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
