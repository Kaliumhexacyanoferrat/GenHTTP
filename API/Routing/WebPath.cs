namespace GenHTTP.Api.Routing;

/// <summary>
/// Specifies a resource available on the server.
/// </summary>
public sealed class WebPath(IReadOnlyList<WebPathPart> parts, bool trailingSlash)
{

    #region Initialization

    public static WebPath FromString(string path)
    {
        var pathSpan = path.AsSpan();
        
        var parts = new List<WebPathPart>(4);

        var start = 0;
        var trailingSlash = false;
        
        for (var i = 0; i <= pathSpan.Length; i++)
        {
            if (i != pathSpan.Length && pathSpan[i] != '/')
                continue;

            if (i > start)
            {
                parts.Add(new WebPathPart(pathSpan[start..i].ToString()));
            }
                
            start = i + 1;
                
            if (i == pathSpan.Length - 1)
            {
                trailingSlash = true;
            }
        }

        return new WebPath(parts, trailingSlash);
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
