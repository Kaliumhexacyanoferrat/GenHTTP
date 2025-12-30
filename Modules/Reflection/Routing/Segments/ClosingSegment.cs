using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection.Routing.Segments;

/// <summary>
/// Appended to every operation to check, whether a trailing slash
/// has been requested and enables wildcards for routes.
/// </summary>
/// <param name="forceTrailingSlash">If set to true, the segment will only match if a trailing slash has been passed with the request</param>
/// <param name="wildcard">If set to false, the segment will not match if additional path segments have been passed</param>
public sealed class ClosingSegment(bool forceTrailingSlash, bool wildcard) : IRoutingSegment
{

    public string[] ProvidedArguments { get; } = [];

    public (bool matched, int offsetBy) TryMatch(RoutingTarget target, int offset, ref PathArgumentSink argumentSink)
    {
        var ended = target.Next(offset) is null;

        if (!ended)
        {
            return wildcard ? (true, 0) : (false, 0);
        }

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

}
