using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core
{

    public static class HandlerExtensions
    {

        public static IResponseBuilder MethodNotAllowed(this IHandler handler, IRequest request, string? title = null, string? message = null)
        {
            var actualTitle = title ?? "Method Not Allowed";
            var actualMessage = message ?? "The specified resource cannot be accessed with the given HTTP verb.";

            var model = new ErrorModel(request, ResponseStatus.MethodNotAllowed, actualTitle, actualMessage, null);

            return handler.Error(model);
        }

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

            while (true)
            {
                if (current is T found)
                {
                    return found;
                }

                if (current == root)
                {
                    return null;
                }

                current = current.Parent;
            }
        }

        public static WebPath GetRoot(this IHandler handler, IHandler root, bool trailingSlash)
        {
            var path = new PathBuilder(trailingSlash);

            var current = handler;

            IHandler? child = null;

            while (true)
            {
                if (current is IRootPathAppender appender)
                {
                    appender.Append(path, child);
                }

                if (current == root)
                {
                    return path.Build();
                }

                child = current;
                current = current.Parent;
            }
        }

        public static IEnumerable<ContentElement> GetContent(this IHandler handler, IRequest request, string title, ContentType contentType)
        {
            return new List<ContentElement>()
            {
                new ContentElement(GetRoot(handler, request.Server.Handler, false), title, contentType, null)
            };
        }
        
        public static IEnumerable<ContentElement> GetContent(this IHandler handler, IRequest request, string title, FlexibleContentType contentType)
        {
            return new List<ContentElement>()
            {
                new ContentElement(GetRoot(handler, request.Server.Handler, false), title, contentType, null)
            };
        }

    }

}
