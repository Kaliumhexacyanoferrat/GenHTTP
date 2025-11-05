using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

public static class ResponseTypeExtensions
{

    /// <summary>
    /// Specifies the content type of this response.
    /// </summary>
    /// <param name="contentType">The content type of this response</param>
    public static IResponseBuilder Type(this IResponseBuilder builder, ContentType contentType) => builder.Type(FlexibleContentType.Get(contentType));

    /// <summary>
    /// Specifies the content type of this response.
    /// </summary>
    /// <param name="contentType">The content type of this response</param>
    public static IResponseBuilder Type(this IResponseBuilder builder, string contentType) => builder.Type(FlexibleContentType.Parse(contentType));


}
