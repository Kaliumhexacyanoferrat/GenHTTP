using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO
{

    public static class ResourceTreeExtensions
    {

        /// <summary>
        /// Attempts to resolve the requested node and/or resource from
        /// the given container, according to the specified routing target.
        /// </summary>
        /// <param name="node">The node used to resolve the target</param>
        /// <param name="target">The target to be resolved</param>
        /// <returns>A tuple of the node and resource resolved from the container (or both null, if they could not be resolved)</returns>
        public async static ValueTask<(IResourceContainer? node, IResource? resource)> Find(this IResourceContainer node, RoutingTarget target)
        {
            var current = target.Current;

            if (current is not null)
            {
                IResourceNode? childNode;

                if (target.Last)
                {
                    IResource? resource;

                    if ((resource = await node.TryGetResourceAsync(current.Value)) != null)
                    {
                        return (node, resource);
                    }
                    else if ((childNode = await node.TryGetNodeAsync(current.Value)) != null)
                    {
                        return (childNode, null);
                    }

                    return (null, null);
                }
                else
                {
                    if ((childNode = await node.TryGetNodeAsync(current.Value)) != null)
                    {
                        target.Advance();
                        return await childNode.Find(target);
                    }
                }
            }

            return new(node, null);
        }

        /// <summary>
        /// Enumerates the content of the container.
        /// </summary>
        /// <param name="node">The node to compute the content for</param>
        /// <param name="request">The currently executed request</param>
        /// <param name="handler">The handler which provides the content</param>
        /// <returns>The content discovered from the given node</returns>
        public static IAsyncEnumerable<ContentElement> GetContent(this IResourceContainer node, IRequest request, IHandler handler)
        {
            return node.GetContent(request, handler, async (_, path, children) =>
            {
                return new ContentElement(path, ContentInfo.Empty, ContentType.ApplicationForceDownload, await children.ToEnumerableAsync());
            });
        }

        /// <summary>
        /// Enumerates the content of the container.
        /// </summary>
        /// <param name="node">The node to compute the content for</param>
        /// <param name="request">The currently executed request</param>
        /// <param name="handler">The handler which provides the content</param>
        /// <param name="indexProvider">A function to provide additional information about the content provided when nodes are requested</param>
        /// <returns>The content discovered from the given node</returns>
        public static async IAsyncEnumerable<ContentElement> GetContent(this IResourceContainer node, IRequest request, IHandler handler, Func<IResourceContainer, WebPath, IAsyncEnumerable<ContentElement>, ValueTask<ContentElement>> indexProvider)
        {
            var path = node.GetPath(request, handler);

            await foreach (var childNode in node.GetNodes())
            {
                var nodePath = path.Edit(false)
                                   .Append(childNode.Name)
                                   .Build();

                yield return await indexProvider(node, nodePath, childNode.GetContent(request, handler, indexProvider));
            }

            await foreach (var resource in node.GetResources())
            {
                var name = resource.Name;

                if (name is not null)
                {
                    var resourcePath = path.Edit(false)
                                           .Append(name)
                                           .Build();

                    var contentType = resource.ContentType ?? FlexibleContentType.Get(name.GuessContentType() ?? ContentType.ApplicationForceDownload);

                    yield return new ContentElement(resourcePath, ContentInfo.Empty, contentType);
                }
            }
        }

        /// <summary>
        /// Fetches the path of the node.
        /// </summary>
        /// <param name="node">The node to determine the path for</param>
        /// <param name="request">The currently executed request</param>
        /// <param name="handler">The handler the node is served by</param>
        /// <returns>The path of the node on this server instance</returns>
        public static WebPath GetPath(this IResourceContainer node, IRequest request, IHandler handler)
        {
            var segments = new List<string>();

            var current = node;

            while (current is IResourceNode currentNode)
            {
                segments.Add(currentNode.Name);

                current = currentNode.Parent;
            }

            segments.Reverse();

            var path = handler.GetRoot(request, true)
                              .Edit(true);

            foreach (var segment in segments)
            {
                path.Append(segment);
            }

            return path.Build();
        }

        private static async ValueTask<IEnumerable<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> enumeration)
        {
            var result = new List<T>();

            await foreach (var element in enumeration)
            {
                result.Add(element);
            }

            return result;
        }

    }

}
