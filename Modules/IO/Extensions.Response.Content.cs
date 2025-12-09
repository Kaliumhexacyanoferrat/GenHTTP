namespace GenHTTP.Modules.IO;

using GenHTTP.Api.Content.IO;
using Api.Protocol;

using Streaming;

using StreamContent = GenHTTP.Modules.IO.Streaming.StreamContent;

public static class ResponseContentExtensions
{
    private static readonly FlexibleContentType TextPlainType = new(ContentType.TextPlain, "UTF-8");

    /// <summary>
    /// Sends the given string to the client.
    /// </summary>
    /// <param name="text">The string to be sent</param>
    public static IResponseBuilder Content(this IResponseBuilder builder, string text) => builder.Content(Resource.FromString(text).Type(TextPlainType).Build());

    /// <summary>
    /// Sends the given resource to the client.
    /// </summary>
    /// <param name="resource">The resource to be sent</param>
    /// <remarks>
    /// This method will set the content, but not the content
    /// type of the response.
    /// </remarks>
    public static IResponseBuilder Content(this IResponseBuilder builder, IResource resource) => builder.Content(new ResourceContent(resource)).Type(resource.ContentType ?? FlexibleContentType.Get(ContentType.ApplicationOctetStream));

    /// <summary>
    /// Sends the given stream to the client.
    /// </summary>
    /// <param name="stream">The stream to be sent</param>
    /// <param name="knownLength">The known length of the stream (if the stream does not propagate this information)</param>
    /// <param name="checksumProvider">The logic to efficiently calculate checksums</param>
    public static IResponseBuilder Content(this IResponseBuilder builder, Stream stream, ulong? knownLength = null, Func<ValueTask<ulong?>>? checksumProvider = null) => builder.Content(new StreamContent(stream, knownLength, checksumProvider));

    /// <summary>
    /// Sends the given binary data to the client.
    /// </summary>
    /// <param name="data">The data to be sent</param>
    /// <param name="checksumProvider">The logic to efficiently calculate checksums</param>
    public static IResponseBuilder Content(this IResponseBuilder builder, byte[] data, Func<ValueTask<ulong?>>? checksumProvider = null) => builder.Content(new ByteArrayContent(data, checksumProvider));

    /// <summary>
    /// Sends the given binary data to the client.
    /// </summary>
    /// <param name="data">The data to be sent</param>
    /// <param name="checksumProvider">The logic to efficiently calculate checksums</param>
    public static IResponseBuilder Content(this IResponseBuilder builder, ReadOnlyMemory<byte> data, Func<ValueTask<ulong?>>? checksumProvider = null) => builder.Content(new MemoryContent(data, checksumProvider));

}
