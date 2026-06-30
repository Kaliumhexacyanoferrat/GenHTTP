namespace GenHTTP.Api.Protocol;

/// <summary>
/// A specific piece of content that can be sent to
/// a connected client in the HTTP response body.
/// </summary>
public interface IResponseContent
{

    /// <summary>
    /// The length of the response body.
    /// </summary>
    /// <remarks>
    /// If this property returns null, the server will apply
    /// chunked transfer encoding. Therefore, try to provide the
    /// exact length of the response, if known. The length needs
    /// to match the actual number of bytes being written in
    /// <see cref="WriteAsync"/> or the client connection
    /// will fail.
    /// </remarks>
    ulong? Length { get; }

    /// <summary>
    /// The type of content sent to the client.
    /// </summary>
    ContentType? Type { get; }

    /// <summary>
    /// The encoding of the content, if any (such as "br").
    /// </summary>
    ReadOnlyMemory<byte>? Encoding { get; }

    /// <summary>
    /// Calculates a checksum for the content.
    /// </summary>
    /// <remarks>
    /// Used by several handlers and concerns to check, whether the
    /// content has changed. Implementation should be efficient as
    /// possible while still noticing changes.
    /// </remarks>
    /// <returns>The checksum of the content</returns>
    ValueTask<ulong?> CalculateChecksumAsync();

    /// <summary>
    /// Writes the content to the given sink.
    /// </summary>
    /// <param name="sink">The sink to write the content to</param>
    ValueTask WriteAsync(IResponseSink sink);

}
