using System.Buffers;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Context;

namespace GenHTTP.Engine.Internal.Protocol.Sinks;

internal sealed class RegularSink(ClientContext context) : IResponseSink
{
    private ContextStream? _stream;

    public IBufferWriter<byte> Writer => context.Writer;

    public Stream Stream => _stream ??= new ContextStream(Writer);

    public void Apply()
    {
        _stream = null;
    }

}
