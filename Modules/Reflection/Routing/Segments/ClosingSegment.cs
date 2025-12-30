using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection.Routing.Segments;

public class ClosingSegment(bool forceTrailingSlash, bool wildcard) : IRoutingSegment
{

    public string[] ProvidedArguments { get; } = [];

    public (bool matched, int offsetBy) TryMatch(RoutingTarget target, int offset, Action<string, string> pathParamCallback)
    {
        var ended = target.Next(offset) is null;

        if (ended)
        {
            var endedWithSlash = target.Path.TrailingSlash;

            if (endedWithSlash)
            {
                return (true, 0);
            }

            if (forceTrailingSlash)
            {
                return (false, 0);
            }

            return (true, offset > 0 ? -1 : 0);
        }

        return wildcard ? (true, 0) : (false, 0);
    }

}
