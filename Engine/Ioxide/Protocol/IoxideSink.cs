using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol;

internal sealed class IoxideSink(PipeWriter writer) : IResponseSink
{
    private Stream? _stream;

    public IBufferWriter<byte> Writer => writer;

    public Stream Stream => _stream ??= new PipeWriterStream(writer, writer);
}
