using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection.Routing.Segments;

/// <summary>
/// Matches a single segment within a requested path, such as "/segment/".
/// </summary>
/// <param name="segment">The segment to match</param>
internal class StringSegment(string segment) : IRoutingSegment
{
    
    public string[] ProvidedArguments { get; } = [];

    public (bool matched, int offsetBy) TryMatch(RoutingTarget target, int offset, ref PathArgumentSink argumentSink)
        => (target.Next(offset)?.Value == segment, 1);

}
