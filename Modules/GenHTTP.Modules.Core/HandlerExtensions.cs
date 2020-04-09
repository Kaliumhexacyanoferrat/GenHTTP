using System;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core
{

    public static class HandlerExtensions
    {

        public static IResponseBuilder NotFound(this IHandler handler, IRequest request, string? title = null, string? message = null)
        {
            var actualTitle = title ?? "Not Found";
            var actualMessage = message ?? "The specified resource could not be found on this server.";

            var model = new ErrorModel(request, ResponseStatus.NotFound, actualTitle, actualMessage, null);

            return handler.Error(model);
        }

        public static IResponseBuilder Error(this IHandler handler, ErrorModel model)
        {
            var renderer = handler.FindParent<IErrorHandler>(model.Request.Server.Handler) ?? throw new InvalidOperationException("There is no error handler available in the routing tree");

            return handler.Page(renderer.Render(model))
                          .Status(model.Status);
        }

        public static IResponseBuilder Page(this IHandler handler, TemplateModel model)
        {
            var renderer = handler.FindParent<IPageRenderer>(model.Request.Server.Handler) ?? throw new InvalidOperationException("There is no page renderer available in the routing tree");

            return renderer.Render(model);
        }

        public static T? FindParent<T>(this IHandler handler, IHandler root) where T : class
        {
            var current = handler;

            while (current != root)
            {
                if (current is T found)
                {
                    return found;
                }

                current = current.Parent;
            }

            return null;
        }

    }

}
