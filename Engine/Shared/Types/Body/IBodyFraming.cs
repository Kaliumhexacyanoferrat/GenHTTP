namespace GenHTTP.Engine.Shared.Types.Body;

/// <summary>
/// Tracks the position within the body of a request as declared by its
/// framing, so that the body can be read or skipped from wherever the
/// consumer left off.
/// </summary>
internal interface IBodyFraming
{

    #region Get-/Setters

    /// <summary>
    /// The declared length of the body, if known up front.
    /// </summary>
    long? Length { get; }

    /// <summary>
    /// The number of body bytes handed out so far.
    /// </summary>
    long Position { get; }

    #endregion

    #region Functionality

    /// <summary>
    /// Copies the next bytes of the body into the given buffer.
    /// </summary>
    /// <returns>The number of bytes copied, zero if the body has been read completely</returns>
    ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the rest of the body into a single memory view.
    /// </summary>
    /// <remarks>
    /// The view may point into the buffer of the connection, so it stays
    /// valid only until the body is skipped.
    /// </remarks>
    ValueTask<ReadOnlyMemory<byte>> ReadToEndAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Skips the rest of the body so that the connection is positioned
    /// at the start of the next request.
    /// </summary>
    /// <returns>false, if the body ended early or was malformed, so the connection cannot be reused</returns>
    ValueTask<bool> SkipAsync();

    /// <summary>
    /// Detaches the framing from the connection so it can be reused.
    /// </summary>
    void Reset();

    #endregion

}
