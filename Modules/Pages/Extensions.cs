using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Pages
{

    public static class Extensions
    {
        private static readonly FlexibleContentType _ContentType = new(ContentType.TextHtml, "utf-8");

        public static IResponseBuilder GetPage(this IRequest request, string content)
        {
            var resource = Resource.FromString(content)
                                   .Build();

            return request.Respond()
                          .Content(new ResourceContent(resource))
                          .Type(_ContentType);
        }

    }

}
