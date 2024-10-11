using System.Text.RegularExpressions;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection;

public sealed class MethodRouting
{
    private readonly string _PathExpression;

    private Regex? _ParsedPath;

    #region Initialization

    public MethodRouting(string pathExpression, bool isIndex, bool isWildcard)
    {
        _PathExpression = pathExpression;

        IsIndex = isIndex;
        IsWildcard = isWildcard;
    }

    #endregion

    #region Get-/Setters

    /// <summary>
    /// The path of the method, converted into a regular
    /// expression to be evaluated at runtime.
    /// </summary>
    public Regex ParsedPath => _ParsedPath ??= new Regex(_PathExpression, RegexOptions.Compiled);

    /// <summary>
    /// True, if this route matches the index of the
    /// scoped segment.
    /// </summary>
    public bool IsIndex { get; }

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
    public bool IsWildcard { get; }

    #endregion

}
