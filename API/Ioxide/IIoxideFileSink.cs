namespace GenHTTP.Api.Ioxide;

/// <summary>
/// A response sink that appends part of a file to the body itself, reading it off the ring straight into the
/// connection's write buffer, after the headers already there.
/// </summary>
/// <remarks>
/// Offered where that buffer is what the response is written to: HTTP/1.1 in cleartext, or with kernel TLS, where
/// the kernel encrypts on send. Content that serves a file checks the sink for this first, and copies the file
/// through a buffer of its own otherwise.
/// </remarks>
public interface IIoxideFileSink
{

    /// <summary>
    /// Appends <paramref name="length"/> bytes of the file <paramref name="fd"/>, from <paramref name="offset"/>, to the
    /// body.
    /// </summary>
    ValueTask WriteFileAsync(int fd, long offset, long length);

}
