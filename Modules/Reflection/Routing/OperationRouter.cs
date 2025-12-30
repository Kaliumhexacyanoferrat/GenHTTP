using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection.Routing;

internal static class OperationRouter
{

    internal static RoutingMatch? TryMatch(RoutingTarget target, OperationRoute route)
    {
        var segments = route.Segments;

        Dictionary<string, string>? pathArguments = null;

        var offset = 0;

        for (var i = 0; i < segments.Count; i++)
        {
            var current = segments[i];

            var (matched, offsetBy) = current.TryMatch(target, i, (key, value) => (pathArguments ??= []).Add(key, value));

            if (!matched)
            {
                return null;
            }
            
            offset += offsetBy;
        }

        return new(offset, pathArguments);
    }

}
