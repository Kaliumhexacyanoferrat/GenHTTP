using System;
using System.IO;
using System.Text;

using GenHTTP.Api.Protocol;

using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Modules.Core.Templating
{

    public static class TemplatingExtensions
    {

        public static IResponseBuilder Content(this IResponseBuilder response, TemplateModel model)
        {
            if (response.Request.Routing == null)
            {
                throw new InvalidOperationException("Routing context is not available");
            }

            var content = response.Request.Routing.Router
                                          .GetRenderer()
                                          .Render(model);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            return response.Content(stream, ContentType.TextHtml);
        }

    }

}
