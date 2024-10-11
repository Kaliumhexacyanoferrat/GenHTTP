namespace GenHTTP.Api.Protocol;

/// <summary>
/// Represents the content of a HTTP response to be sent to the client.
/// </summary>
/// <remarks>
/// Allows to efficiently stream data into the network stream used by the server.
/// </remarks>
public interface IResponseContent
{
        
    /// <summary>
    /// The number of bytes to be sent to the client (if known).
    /// </summary>
    /// <remarks>
    /// If <code>null</code> is returned by this method, the server needs
    /// to use chunked encoding to send the data to the client. Therefore,
    /// try to determine the correct length of the content to be sent
    /// whenever possible.
    /// 
    /// Writing more or less bytes than indicated by this property to the
    /// target stream will cause HTTP client errors or timeouts to occur.
    /// </remarks>
    ulong? Length { get; }

    /// <summary>
    /// A checksum of the content represented by this instance. 
    /// </summary>
    /// <remarks>
    /// The checksum calculation should be as fast as possible but
    /// still allow to reliably detect changes. For efficient processing,
    /// this also means that the content should actually be expanded
    /// when the Write call is invoked, not when the content instance
    /// is constructed.
    /// </remarks>
    ValueTask<ulong?> CalculateChecksumAsync();

    /// <summary>
    /// Writes the content to the specified target stream.
    /// </summary>
    /// <param name="target">The stream to write the data to</param>
    /// <param name="bufferSize">The buffer size to be used to write the data</param>
    ValueTask WriteAsync(Stream target, uint bufferSize);

}
