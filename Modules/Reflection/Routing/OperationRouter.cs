using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection.Routing;

internal static class OperationRouter
{

    /// <summary>
    /// Checks whether the requested path matches the given route.
    /// </summary>
    /// <param name="target">The requested path</param>
    /// <param name="route">The route to check for</param>
    /// <returns>A match if the route is capable of handling the incoming request</returns>
    internal static RoutingMatch? TryMatch(RoutingTarget target, OperationRoute route)
    {
        var segments = route.Segments;
        
        var sink = new PathArgumentSink();

        var offset = 0;

        for (var i = 0; i < segments.Count; i++)
        {
            var current = segments[i];

            var (matched, offsetBy) = current.TryMatch(target, i, ref sink);

            if (!matched)
            {
                return null;
            }
            
            offset += offsetBy;
        }

        return new(offset, sink.Arguments);
    }
    
}
