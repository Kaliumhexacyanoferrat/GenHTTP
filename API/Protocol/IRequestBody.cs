namespace GenHTTP.Api.Protocol;

public interface IRequestBody
{

    /// <summary>
    /// Provides the body of the request as a stream.
    /// </summary>
    /// <returns>The stream created to access the body</returns>
    Stream AsStream();

    /// <summary>
    /// Reads the whole request body into a byte array.
    /// </summary>
    /// <returns>The byte array read from the request body</returns>
    byte[] AsArray();

}
