using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Sinks;

/// <summary>The response sink for a body whose length is not known up front.</summary>
// Not Shared's Http1ChunkedSink, which builds a WritingStream - see Http1WriterStream.
internal sealed class Http1ChunkedSink : IResponseSink
{
    private readonly PipeWriter _writer;

    private readonly Http1ChunkedWriter _chunked;

    private Stream? _stream;

    // Wraps the connection's writer in chunk framing.
    public Http1ChunkedSink(PipeWriter writer)
    {
        _writer = writer;
        _chunked = new Http1ChunkedWriter(writer);
    }

    public IBufferWriter<byte> Writer => _chunked;

    public Stream Stream => _stream ??= new Http1WriterStream(_chunked, _writer);

    // Closes the chunked body once the content is written.
    public void Finish() => _chunked.Finish();
}
