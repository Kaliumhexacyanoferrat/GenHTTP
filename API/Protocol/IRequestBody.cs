namespace GenHTTP.Api.Protocol;

/// <summary>
/// Grants access to the body of the HTTP request.
/// </summary>
public interface IRequestBody
{

    /// <summary>
    /// Reads the content of the body as a stream.
    /// </summary>
    /// <returns>The stream representing the body of the request</returns>
    Stream AsStream();

    /// <summary>
    /// Reads the content of the body as a memory view.
    /// </summary>
    /// <returns>The memory view representing the body of the request</returns>
    ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync();

}
