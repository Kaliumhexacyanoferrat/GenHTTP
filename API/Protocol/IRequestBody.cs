namespace GenHTTP.Api.Protocol;

public interface IRequestBody
{

    /// <summary>
    /// Fetches the next chunk of data from the underlying connection.
    /// </summary>
    /// <returns>The next chunk of data read from the underlying connection or null, if no more data is left to be read</returns>
    /// <remarks>
    /// Transparently handles content length boundaries and chunked encoding.
    /// </remarks>
    ValueTask<ReadOnlyMemory<byte>?> TryReadAsync();

    // todo: have a length here? pattern similar to to the response body

}
