using System.Text.RegularExpressions;

namespace GenHTTP.Modules.Reflection.Operations;

public sealed class OperationPath
{

    #region Get-/Setters

    /// <summary>
    /// An user-friendly string to display this path.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The path of the method, converted into a regular
    /// expression to be evaluated at runtime.
    /// </summary>
    public Regex Matcher { get; }

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

    #region Initialization

    public OperationPath(string name, Regex matcher, bool isIndex, bool isWildcard)
    {
        Matcher = matcher;
        Name = name;
        IsIndex = isIndex;
        IsWildcard = isWildcard;
    }

    #endregion

}
