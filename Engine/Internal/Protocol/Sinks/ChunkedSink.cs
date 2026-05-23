using System.Buffers;

using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Context;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Engine.Internal.Protocol.Sinks;

internal sealed class ChunkedSink(ClientContext context) : IResponseSink
{
    private readonly ChunkedWriter _writer = new(context);

    private WritingStream? _stream;

    public IBufferWriter<byte> Writer => _writer;

    public Stream Stream => _stream ??= new WritingStream(context.Writer, context.Stream);

    public void Apply()
    {
        _stream = null;
    }

    public void Finish() => _writer.Finish();

}
