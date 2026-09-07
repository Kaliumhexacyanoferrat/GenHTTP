namespace GenHTTP.Api.Protocol;

/// <summary>
/// The target resource requested by the client. Enables
/// routing by storing the processing state across handlers.
/// </summary>
public interface IRequestTarget
{

    /// <summary>
    /// The current segment that needs to be routed by
    /// the middleware.
    /// </summary>
    /// <remarks>
    /// Acts as a pointer on the routing target that can
    /// be moved forward via <see cref="Advance" />. Handlers
    /// that are concerned with routing will analyze the current
    /// value and decide their action based on it. Afterward, they
    /// will advance the target, so child handlers will not see
    /// the path prefix.
    /// </remarks>
    PathSegment? Current { get; }

    /// <summary>
    /// Specifies whether this is the last segment to be handled.
    /// </summary>
    bool IsLast { get; }

    /// <summary>
    /// Specifies, whether the request path ends with a trailing
    /// slash ('/').
    /// </summary>
    bool HasTrailingSlash { get; }

    /// <summary>
    /// Advances the routing pointer by the given number of segments.
    /// </summary>
    /// <param name="segments">The number of segments to advance by</param>
    void Advance(int segments = 1);

    /// <summary>
    /// Peeks at the next segment with the given offset (zero-based).
    /// </summary>
    /// <param name="offset">The offset of the segment to read</param>
    /// <returns>The path segment at the specified offset</returns>
    PathSegment? Next(int offset);

    /// <summary>
    /// Creates a new routing target with the given suffix appended.
    /// </summary>
    /// <remarks>
    /// For example, you can use this method to add a file extension
    /// to an existing path (such as appending '.gz' to 'file.txt').
    /// The resulting request target will have the same routing state
    /// as this instance.
    /// </remarks>
    /// <param name="suffix">The suffix to be appended to the target</param>
    /// <returns>The newly created copy</returns>
    IRequestTarget CopyAndAppend(ReadOnlyMemory<byte> suffix);

    /// <summary>
    /// Creates a string representation of the request target.
    /// </summary>
    /// <param name="decode">Whether URL encoded characters should be decoded</param>
    /// <param name="remainingOnly">Whether the whole path or only the non-routed parts should be included</param>
    /// <returns>The requested string representation</returns>
    string AsString(bool decode = true, bool remainingOnly = false);

}
