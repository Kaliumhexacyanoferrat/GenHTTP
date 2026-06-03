namespace GenHTTP.Api.Protocol;

/// <summary>
/// Grants access to the body of the HTTP request.
/// </summary>
public interface IRequestBody
{

    Stream AsStream();

    ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync();

}
