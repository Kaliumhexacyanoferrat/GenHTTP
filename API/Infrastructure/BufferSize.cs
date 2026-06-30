namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// Definition of the buffer sizes used by the engines.
/// </summary>
/// <remarks>
/// Use this buffer sizes in performance critical, custom
/// handler implementations to match the buffer sizes used
/// by the engines. This avoids unnecessary copying and resizing
/// of buffers.
/// </remarks>
public static class BufferSize
{

    /// <summary>
    /// The buffer size used to read incoming requests
    /// from the underlying connection.
    /// </summary>
    /// <remarks>
    /// The read buffer size is chosen to ensure that almost
    /// any request can be read in one continuous block from
    /// the OS buffer, allowing it to be parsed synchronously
    /// without the need of an additional, async read.
    /// </remarks>
    public const int Read = 16 * 1024;

    /// <summary>
    /// The buffer size used to write response data
    /// to the underlying connection.
    /// </summary>
    /// <remarks>
    /// The write buffer size aligns with the typical OS socket
    /// buffer size to avoid too many write operations while also
    /// avoiding the write buffer from being split when sending data.
    /// </remarks>
    public const int Write = 64 * 1024;

}
