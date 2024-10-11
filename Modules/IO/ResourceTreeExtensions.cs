using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.IO;

public static class ResourceTreeExtensions
{

    /// <summary>
    /// Attempts to resolve the requested node and/or resource from
    /// the given container, according to the specified routing target.
    /// </summary>
    /// <param name="node">The node used to resolve the target</param>
    /// <param name="target">The target to be resolved</param>
    /// <returns>A tuple of the node and resource resolved from the container (or both null, if they could not be resolved)</returns>
    public static async ValueTask<(IResourceContainer? node, IResource? resource)> Find(this IResourceContainer node, RoutingTarget target)
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
                if ((childNode = await node.TryGetNodeAsync(current.Value)) != null)
                {
                    return (childNode, null);
                }

                return (null, null);
            }
            if ((childNode = await node.TryGetNodeAsync(current.Value)) != null)
            {
                target.Advance();
                return await childNode.Find(target);
            }
        }

        return new ValueTuple<IResourceContainer?, IResource?>(node, null);
    }
}
