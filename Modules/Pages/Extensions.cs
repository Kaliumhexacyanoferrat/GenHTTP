using System.Web;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Strings;

namespace GenHTTP.Modules.Pages
{

    public static class Extensions
    {
        private static readonly FlexibleContentType _ContentType = new(ContentType.TextHtml, "utf-8");

        /// <summary>
        /// Creates a response that can be returned by a handler to serve
        /// a HTML page.
        /// </summary>
        /// <param name="request">The request to be responded to</param>
        /// <param name="content">The HTML page to be served</param>
        /// <returns>The HTML page response</returns>
        public static IResponseBuilder GetPage(this IRequest request, string content)
        {
            return request.Respond()
                          .Content(new StringContent(content))
                          .Type(_ContentType);
        }

        /// <summary>
        /// Escapes the given string so it can safely be used in HTML.
        /// </summary>
        /// <param name="content">The content to be escaped</param>
        /// <returns>The escaped version of the string</returns>
        public static string Escaped(this string content) => HttpUtility.HtmlEncode(content);

    }

}
