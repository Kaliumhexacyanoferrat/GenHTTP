using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Pages.Rendering;

namespace GenHTTP.Modules.Pages
{

    public static class Extensions
    {
        private static readonly FlexibleContentType _HtmlType = new(ContentType.TextHtml, "UTF-8");

        public static IResponseBuilder GetMethodNotAllowed(this IHandler handler, IRequest request, string? title = null, string? message = null)
        {
            var actualMessage = message ?? "The specified resource cannot be accessed with the given HTTP verb.";

            var model = new ErrorModel(request, handler, ResponseStatus.MethodNotAllowed, actualMessage, null);

            var info = ContentInfo.Create()
                                  .Title(title ?? "Method Not Allowed");

            return handler.GetError(model, info.Build());
        }

        public static IResponseBuilder GetNotFound(this IHandler handler, IRequest request, string? title = null, string? message = null)
        {
            var actualMessage = message ?? "The specified resource could not be found on this server.";

            var model = new ErrorModel(request, handler, ResponseStatus.NotFound, actualMessage, null);

            var info = ContentInfo.Create()
                                  .Title(title ?? "Not Found");

            return handler.GetError(model, info.Build());
        }

        public static IResponseBuilder GetError(this IHandler handler, ErrorModel model, ContentInfo details)
        {
            var renderer = handler.FindParent<IErrorRenderer>(model.Request.Server.Handler) ?? throw new InvalidOperationException("There is no error handler available in the routing tree");

            var content = new RenderedContent<ErrorModel>(renderer, model, details);

            return model.Request
                        .Respond()
                        .Content(content)
                        .Type(_HtmlType)
                        .Status(model.Status);
        }
        public static IPageRenderer GetPageRenderer(this IHandler handler, IRequest request)
        {
            return handler.FindParent<IPageRenderer>(request.Server.Handler) ?? throw new InvalidOperationException("There is no page renderer available in the routing tree");
        }

        public static async ValueTask WritePageAsync(this IHandler handler, IRequest request, ContentInfo pageInfo, string content, Stream target)
        {
            var templateModel = new TemplateModel(request, handler, pageInfo, content);

            var templateRenderer = handler.GetPageRenderer(request);

            await templateRenderer.RenderAsync(templateModel, target);
        }

    }

}
