using System;
using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Basics
{

    public static class HandlerExtensions
    {

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

                if (current.Parent == current)
                {
                    throw new InvalidOperationException($"Router '{current}' returned itself as parent");
                }

                current = current.Parent;
            }
        }

        public static WebPath GetRoot(this IHandler handler, IRequest request, bool trailingSlash)
        {
            var path = new PathBuilder(trailingSlash);

            var current = handler;

            var root = request.Server.Handler;

            IHandler? child = null;

            while (true)
            {
                if (current is IRootPathAppender appender)
                {
                    appender.Append(path, request, child);
                }

                if (current == root)
                {
                    return path.Build();
                }

                if (current.Parent == current)
                {
                    throw new InvalidOperationException($"Router '{current}' returned itself as parent");
                }

                child = current;
                current = current.Parent;
            }
        }

        public static IAsyncEnumerable<ContentElement> GetContent(this IHandler handler, IRequest request, ContentInfo details, ContentType contentType)
        {
            return new List<ContentElement>() { new ContentElement(handler.GetRoot(request, false), details, contentType, null) }.ToAsyncEnumerable();
        }

        public static IAsyncEnumerable<ContentElement> GetContent(this IHandler handler, IRequest request, ContentInfo details, FlexibleContentType contentType)
        {
            return new List<ContentElement>() { new ContentElement(handler.GetRoot(request, false), details, contentType, null) }.ToAsyncEnumerable();
        }

        public static string? Route(this IHandler handler, IRequest request, string? route, bool relative = true)
        {
            if (route is not null)
            {
                if (route.StartsWith("http") || route.StartsWith('/'))
                {
                    return route;
                }

                if (route.StartsWith("."))
                {
                    if (!relative)
                    {
                        var routePath = new PathBuilder(route).Build();

                        return request.Target.Path.Combine(routePath)
                                                  .ToString();
                    }

                    return route;
                }

                var root = request.Server.Handler;

                var parts = route.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => new WebPathPart(p))
                                 .ToList();

                if (parts.Count > 0)
                {
                    var target = parts[0];

                    foreach (var resolver in handler.FindParents<IHandlerResolver>(root))
                    {
                        var responsible = resolver.Find(target.Value);

                        if (responsible is not null)
                        {
                            var targetParts = responsible.GetRoot(request, false)
                                                         .Edit(route.EndsWith('/'));

                            for (int i = 1; i < parts.Count; i++)
                            {
                                targetParts.Append(parts[i]);
                            }

                            var targetPath = targetParts.Build();

                            if (relative)
                            {
                                return request.Target.Path.RelativeTo(targetPath);
                            }

                            return targetPath.ToString();
                        }
                    }
                }
            }

            return null;
        }

        public static string? Route(this IHandler handler, IRequest request, WebPath? route)
        {
            if (route is not null)
            {
                return handler.Route(request, route.ToString());
            }

            return null;
        }

        public static string? Route(this IHandler handler, IRequest request, WebPathPart? part)
        {
            if (part is not null)
            {
                return handler.Route(request, part.Original);
            }

            return null;
        }

        public static IEnumerable<T> FindParents<T>(this IHandler handler, IHandler root)
        {
            var current = handler;

            while (true)
            {
                if (current is T found)
                {
                    yield return found;
                }

                if (current == root)
                {
                    break;
                }

                if (current.Parent == current)
                {
                    throw new InvalidOperationException($"Router '{current}' returned itself as parent");
                }

                current = current.Parent;
            }
        }


    }

}
