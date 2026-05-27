namespace GenHTTP.Api.Protocol;

/// <summary>Represents an HTTP protocol version.</summary>
[MemoryView]
public readonly partial struct HttpProtocol
{

    #region Known Versions

    public static readonly HttpProtocol Http10 = new("HTTP/1.0"u8.ToArray());
    public static readonly HttpProtocol Http11 = new("HTTP/1.1"u8.ToArray());

    // well, todo
    public static readonly HttpProtocol Http2  = new("HTTP/2.0"u8.ToArray());
    public static readonly HttpProtocol Http3  = new("HTTP/3.0"u8.ToArray());

    #endregion

}
