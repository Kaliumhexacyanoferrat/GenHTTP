using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO
{

    public static class HandlerExtensions
    {

        public static async ValueTask<IResponseBuilder> GetPageAsync(this IHandler handler, IRequest request, TemplateModel model)
        {
            var templateRenderer = handler.FindParent<IPageRenderer>(request.Server.Handler) ?? throw new InvalidOperationException("There is no page renderer available in the routing tree");

            var content = await templateRenderer.RenderAsync(model);

            var stringResource = Resource.FromString(content)
                                         .Build();

            return request.Respond()
                          .Content(stringResource)
                          .Type(ContentType.TextHtml);
        }

    }


}
