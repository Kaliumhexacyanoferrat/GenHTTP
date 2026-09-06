using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Sinks;

/// <summary>The response sink for a body with a known length: straight into the pipe.</summary>
// Not Shared's RegularSink, which builds a WritingStream - see Http1WriterStream.
internal sealed class Http1Sink(PipeWriter writer) : IResponseSink
{
    private Stream? _stream;

    public IBufferWriter<byte> Writer => writer;

    public Stream Stream => _stream ??= new Http1WriterStream(writer, writer);
}
