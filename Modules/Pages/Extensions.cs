using System.Web;

using GenHTTP.Api.Protocol;

using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Modules.Pages;

public static class Extensions
{
    // todo
    private static readonly ReadOnlyMemory<byte> ContentTypeHeader = "Content-Type"u8.ToArray();
    private static readonly ReadOnlyMemory<byte> ContentTypeValue = "text/html; charset=\"utf-8\""u8.ToArray();

    /// <summary>
    /// Creates a response that can be returned by a handler to serve
    /// a HTML page.
    /// </summary>
    /// <param name="request">The request to be responded to</param>
    /// <param name="content">The HTML page to be served</param>
    /// <returns>The HTML page response</returns>
    public static IResponseBuilder GetPage(this IRequest request, string content) => request.Respond()
                                                                                            .Raw()
                                                                                            .Content(new StringContent(content))
                                                                                            .Header(ContentTypeHeader, ContentTypeValue)
                                                                                            .Unraw();

    /// <summary>
    /// Escapes the given string so it can safely be used in HTML.
    /// </summary>
    /// <param name="content">The content to be escaped</param>
    /// <returns>The escaped version of the string</returns>
    public static string Escaped(this string content) => HttpUtility.HtmlEncode(content);

}
