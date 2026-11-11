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

    /// <summary>
    /// The path arguments provided by the segments of this route,
    /// in the order their values are stored in a <see cref="RoutingMatch" />.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; } = segments.SelectMany(s => s.ProvidedArguments).ToArray();

    /// <summary>
    /// Returns the position of the given path argument (ignoring case),
    /// or null if the route does not provide it.
    /// </summary>
    /// <param name="argument">The name of the argument to look for</param>
    public int? GetArgumentIndex(string argument)
    {
        for (var i = 0; i < Arguments.Count; i++)
        {
            if (string.Equals(Arguments[i], argument, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return null;
    }

}
