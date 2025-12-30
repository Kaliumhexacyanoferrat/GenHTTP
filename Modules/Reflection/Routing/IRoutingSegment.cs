using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection.Routing;

/// <summary>
/// A piece of logic that knows how to check the current segment
/// for a given route and extracts the path arguments from the
/// incoming request.
/// </summary>
public interface IRoutingSegment
{
    
    /// <summary>
    /// The path arguments that are provided by this segment, if any.
    /// </summary>
    string[] ProvidedArguments { get; }

    /// <summary>
    /// Checks whether the segment is responsible for handling the incoming
    /// request.
    /// </summary>
    /// <param name="target">The route of the incoming request</param>
    /// <param name="offset">The offset from the current routing position to be applied</param>
    /// <param name="argumentSink">The sink to write argument values to</param>
    /// <returns>Whether the segment matched and what offset to be applied for this segment</returns>
    (bool matched, int offsetBy) TryMatch(RoutingTarget target, int offset, ref PathArgumentSink argumentSink);
    
}
