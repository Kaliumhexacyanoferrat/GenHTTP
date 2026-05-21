using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

using ByteArrayContent = GenHTTP.Modules.IO.Streaming.ByteArrayContent;
using StreamContent = GenHTTP.Modules.IO.Streaming.StreamContent;
using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Modules.IO;

public static class ResponseContentExtensions
{

    public static IResponseBuilder Content(this IResponseBuilder builder, string text, ContentType? contentType = null) => builder.Content(new StringContent(text, contentType));

    public static IResponseBuilder Content(this IResponseBuilder builder, IResource resource, ContentType? contentType = null) => builder.Content(new ResourceContent(resource, contentType));

    public static IResponseBuilder Content(this IResponseBuilder builder, Stream stream, ContentType? contentType = null, ulong? knownLength = null, ReadOnlyMemory<byte>? encoding = null, Func<ValueTask<ulong?>>? checksumProvider = null) =>
        builder.Content(new StreamContent(stream, contentType, knownLength, encoding, checksumProvider));

    public static IResponseBuilder Content(this IResponseBuilder builder, byte[] data, ContentType? contentType = null, Func<ValueTask<ulong?>>? checksumProvider = null) => builder.Content(new ByteArrayContent(data, contentType, checksumProvider));

    public static IResponseBuilder Content(this IResponseBuilder builder, ReadOnlyMemory<byte> data, ContentType? contentType = null, Func<ValueTask<ulong?>>? checksumProvider = null) => builder.Content(new MemoryContent(data, contentType, checksumProvider));
    
}
