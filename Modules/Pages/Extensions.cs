using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Strings;

namespace GenHTTP.Modules.Pages
{

    public static class Extensions
    {
        private static readonly FlexibleContentType _ContentType = new(ContentType.TextHtml, "utf-8");

        public static IResponseBuilder GetPage(this IRequest request, string content)
        {
            return request.Respond()
                          .Content(new StringContent(content))
                          .Type(_ContentType);
        }

    }

}
