using System.Buffers;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// Exposes channels that can be used by response content
/// instances to write data to the response body.
/// </summary>
public interface IResponseSink
{

    /// <summary>
    /// A writer which can be used to write the response body.
    /// </summary>
    IBufferWriter<byte> Writer { get; }

    /// <summary>
    /// A write-only stream which can be used to write the response body.
    /// </summary>
    Stream Stream { get; }

}
