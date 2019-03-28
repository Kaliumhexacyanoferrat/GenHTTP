using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Protocol;

using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Modules.Core.Templating
{

    public static class TemplatingExtensions
    {

        public static void Send(this IHttpResponse response, TemplateModel model, IHttpRequest origin)
        {
            if (origin.Routing == null)
            {
                throw new InvalidOperationException("Routing context is not available");
            }

            var content = origin.Routing.Router
                                        .GetRenderer()
                                        .Render(model);

            response.Header.ContentType = ContentType.TextHtml;

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                response.Send(stream);
            }
        }

    }

}
