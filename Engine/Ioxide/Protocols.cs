namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// The protocols an endpoint serves. <see cref="Http1"/> and <see cref="Http2"/> share the TCP
/// socket, so enabling both turns no client away; <see cref="Http3"/> is a UDP socket on the same
/// port number, which is what lets one endpoint serve all three.
/// </summary>
[Flags]
public enum Protocols
{
    /// <summary>HTTP/1.1 over TCP.</summary>
    Http1 = 1,

    /// <summary>
    /// HTTP/2 over TCP: by ALPN on a secure endpoint, or by the connection preface (h2c with prior
    /// knowledge) on a plaintext one. The <c>Upgrade:</c> dance is not implemented.
    /// </summary>
    Http2 = 2,

    /// <summary>
    /// HTTP/3 over QUIC, on this endpoint's port number over UDP. Requires a certificate: QUIC
    /// carries TLS 1.3 and has no cleartext mode.
    /// </summary>
    Http3 = 4,

    /// <summary>HTTP/1.1 and HTTP/2 on the TCP socket, chosen per connection.</summary>
    Http1AndHttp2 = Http1 | Http2,

    /// <summary>
    /// HTTP/1.1 over TCP and HTTP/3 over UDP, skipping HTTP/2. Every client is still served, and
    /// HTTP/2's per-connection flow control and HPACK state are not paid for.
    /// </summary>
    Http1AndHttp3 = Http1 | Http3,

    /// <summary>
    /// HTTP/2 over TCP and HTTP/3 over UDP, with no HTTP/1.1 at all. For somewhere the clients are
    /// known - a private API, or gRPC - since anything else is turned away.
    /// </summary>
    Http2AndHttp3 = Http2 | Http3,

    /// <summary>Everything: HTTP/1.1 and HTTP/2 over TCP, HTTP/3 over UDP, one port number.</summary>
    All = Http1 | Http2 | Http3,
}
