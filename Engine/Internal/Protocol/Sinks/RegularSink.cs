using System.Buffers;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Context;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Engine.Internal.Protocol.Sinks;

internal sealed class RegularSink(ClientContext context) : IResponseSink
{
    private WritingStream? _stream;

    public IBufferWriter<byte> Writer => context.Writer;

    public Stream Stream => _stream ??= new WritingStream(Writer);

    public void Apply()
    {
        _stream = null;
    }

}
