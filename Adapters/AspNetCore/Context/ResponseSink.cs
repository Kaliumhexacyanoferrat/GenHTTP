using System.Buffers;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Adapters.AspNetCore.Context;

internal sealed class ResponseSink(IClientContext context) : IResponseSink
{
    private WritingStream? _stream;

    public IBufferWriter<byte> Writer => context.Writer;

    public Stream Stream => _stream ??= new WritingStream(context.Writer, context.Stream);

    public void Apply() => _stream = null;

}
