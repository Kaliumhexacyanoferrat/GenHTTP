using System.Buffers;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Adapters.AspNetCore.Context;

/// <summary>
/// Writes response content to the current <see cref="IClientContext"/> writer/stream.
/// </summary>
/// <remarks>
/// Clone of the (internal, so inaccessible from this project) <c>Sinks.RegularSink</c>
/// in <c>Engine.Shared</c>. Unlike Shared's engines, Kestrel needs only this single sink
/// implementation, not a separate chunked variant: whether or not a Content-Length is
/// known, we just write bytes to the response body - Kestrel applies chunked framing
/// (HTTP/1.1) or DATA frames (h2/h3) itself.
/// </remarks>
internal sealed class ResponseSink(IClientContext context) : IResponseSink
{
    private WritingStream? _stream;

    public IBufferWriter<byte> Writer => context.Writer;

    public Stream Stream => _stream ??= new WritingStream(context.Writer, context.Stream);

    public void Apply() => _stream = null;

}
