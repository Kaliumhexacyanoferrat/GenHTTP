namespace GenHTTP.Modules.Reflection.Routing;

public sealed class OperationRoute(string name, IReadOnlyList<IRoutingSegment> segments, bool isWildcard)
{

    /// <summary>
    /// An user-friendly string to display this path.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// True, if this is a wildcard route that is created
    /// when returning a handler or handler builder from
    /// a method.
    /// </summary>
    /// <remarks>
    /// Wildcard routes have a lower priority compared to
    /// non-wildcard routes and will not be considered
    /// ambiguous.
    /// </remarks>
    public bool IsWildcard { get; } = isWildcard;

    /// <summary>
    /// The segments to be evaluated to check whether the route
    /// has been matched.
    /// </summary>
    public IReadOnlyList<IRoutingSegment> Segments { get; } = segments;

}
