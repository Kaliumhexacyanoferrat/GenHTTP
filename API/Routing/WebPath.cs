namespace GenHTTP.Api.Routing;

/// <summary>
/// Specifies a resource available on the server.
/// </summary>
public sealed class WebPath(IReadOnlyList<WebPathPart> parts, bool trailingSlash)
{

    #region Initialization

    public static WebPath FromString(string path)
    {
        var split = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        var parts = new List<WebPathPart>(split.Length);

        foreach (var entry in split)
        {
            parts.Add(new WebPathPart(entry));
        }

        return new WebPath(parts, path.EndsWith('/'));
    }

    #endregion

    #region Get-/Setters

    /// <summary>
    /// The segments the path consists of.
    /// </summary>
    public IReadOnlyList<WebPathPart> Parts { get; } = parts;

    /// <summary>
    /// Specifies, whether the path ends with a trailing slash.
    /// </summary>
    public bool TrailingSlash { get; } = trailingSlash;

    /// <summary>
    /// Specifies, whether the path equals the root of the server instance.
    /// </summary>
    public bool IsRoot => Parts.Count == 0;

    /// <summary>
    /// The name of the file that is referenced by this path (if this is
    /// the path to a file).
    /// </summary>
    public string? File
    {
        get
        {
            if (!TrailingSlash)
            {
                var part = Parts.Count > 0 ? Parts[^1] : null;

                return part?.Value.Contains('.') ?? false ? part.Value : null;
            }

            return null;
        }
    }

    #endregion

    #region Functionality

    public override string ToString() => ToString(false);

    /// <summary>
    /// Generates the string representation of this path.
    /// </summary>
    /// <param name="encoded">Specifies, whether special characters in the path should be percent encoded</param>
    /// <returns>The string representation of the path</returns>
    public string ToString(bool encoded)
    {
        if (!IsRoot)
        {
            var segments = Parts.Select(p => encoded ? p.Original : p.Value);

            return "/" + string.Join('/', segments) + (TrailingSlash ? "/" : "");
        }

        return "/";
    }

    #endregion

}
