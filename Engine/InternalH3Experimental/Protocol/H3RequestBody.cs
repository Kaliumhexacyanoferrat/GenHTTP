using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.InternalH3Experimental.Protocol;

/// <summary>
/// A request body that Glyph3 has already assembled.
/// </summary>
/// <remarks>
/// Buffered because this engine uses Glyph3's buffered dispatch: the handler runs once the whole
/// request has arrived. Glyph3 also has a streamed flavour, which would replace this with a reader
/// the handler pulls while the body is still in flight.
/// </remarks>
internal sealed class H3RequestBody : IRequestBody
{
    private readonly ReadOnlyMemory<byte> _content;

    internal H3RequestBody(ReadOnlyMemory<byte> content)
    {
        _content = content;
    }

    public Stream AsStream() => new MemoryStream(_content.ToArray(), writable: false);

    public ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync() => new(_content);
}
