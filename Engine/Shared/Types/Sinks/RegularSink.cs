using System.Buffers;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Engine.Shared.Types.Sinks;

internal sealed class RegularSink(IClientContext context) : IResponseSink
{
    private WritingStream? _stream;

    public IBufferWriter<byte> Writer => context.Writer;

    public Stream Stream => _stream ??= new WritingStream(context.Writer, context.Stream);

    public void Apply()
    {
        _stream = null;
    }

}
