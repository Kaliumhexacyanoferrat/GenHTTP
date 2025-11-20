namespace GenHTTP.Api.Routing;

/// <summary>
/// Provides a view on the target path of a request.
/// </summary>
/// <remarks>
/// Stores the state of the routing mechanism and allows handlers to
/// get the remaining parts to be handled.
/// </remarks>
public sealed class RoutingTarget
{
    private static readonly List<WebPathPart> EmptyList = [];

    private int _index;

    #region Initialization

    /// <summary>
    /// Creates a new routing target and sets the pointer to the beginning of the path.
    /// </summary>
    /// <param name="path">The targeted path</param>
    public RoutingTarget(WebPath path)
    {
        Path = path;
    }

    #endregion

    #region Get-/Setters

    /// <summary>
    /// The path of the request to be handled by the server.
    /// </summary>
    public WebPath Path { get; }

    /// <summary>
    /// The segment to be currently handled by the responsible handler.
    /// </summary>
    public WebPathPart? Current => _index < Path.Parts.Count ? Path.Parts[_index] : null;

    /// <summary>
    /// Specifies, whether the end of the path has been reached.
    /// </summary>
    public bool Ended => _index >= Path.Parts.Count;

    /// <summary>
    /// Specifies, whether the last part of the path has been reached.
    /// </summary>
    public bool Last => _index == (Path.Parts.Count - 1);

    #endregion

    #region Functionality

    /// <summary>
    /// Acknowledges the currently handled segment and advances the
    /// pointer to the next one.
    /// </summary>
    public void Advance()
    {
        if (Ended)
        {
            throw new InvalidOperationException("Already at the end of the path");
        }

        _index++;
    }

    /// <summary>
    /// Retrieves the part of the path that still needs to be routed.
    /// </summary>
    /// <returns>The remaining part of the path</returns>
    public WebPath GetRemaining()
    {
        var remaining = Path.Parts.Count - _index;

        var resultList = remaining > 0 ? new List<WebPathPart>(remaining) : EmptyList;

        for (var i = _index; i < Path.Parts.Count; i++)
        {
            resultList.Add(Path.Parts[i]);
        }

        return new WebPath(resultList, Path.TrailingSlash);
    }

    #endregion

}
