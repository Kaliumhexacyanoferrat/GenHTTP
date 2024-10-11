using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;
using StreamContent = GenHTTP.Modules.IO.Streaming.StreamContent;

namespace GenHTTP.Modules.IO;

public static class ResponseBuilderExtensions
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
    public static IResponseBuilder Content(this IResponseBuilder builder, Stream stream, ulong? knownLength, Func<ValueTask<ulong?>> checksumProvider) => builder.Content(new StreamContent(stream, knownLength, checksumProvider));

    /// <summary>
    /// Sends the given stream to the client.
    /// </summary>
    /// <param name="stream">The stream to be sent</param>
    /// <param name="checksumProvider">The logic to efficiently calculate checksums</param>
    public static IResponseBuilder Content(this IResponseBuilder builder, Stream stream, Func<ValueTask<ulong?>> checksumProvider) => builder.Content(stream, null, checksumProvider);

    public static ValueTask<IResponse?> BuildTask(this IResponseBuilder builder) => new(builder.Build());
}
