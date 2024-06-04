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

    }

}
