namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// The protocols an endpoint serves.
/// </summary>
/// <remarks>
/// <see cref="Http1"/> and <see cref="Http2"/> share the TCP socket - which of them a connection
/// gets is settled by ALPN on a secure endpoint and by the connection preface on a plaintext one, so
/// enabling both costs nothing and turns no client away. <see cref="Http3"/> is a UDP socket on the
/// same port number, independent of either.
///
/// <para>That independence is what lets one endpoint serve all three: TCP carries HTTP/1.1 and
/// HTTP/2, UDP carries HTTP/3, and a browser told about the third by an Alt-Svc header moves itself
/// across without changing port.</para>
/// </remarks>
[Flags]
public enum IoxideProtocols
{
    /// <summary>HTTP/1.1 over TCP.</summary>
    Http1 = 1,

    /// <summary>
    /// HTTP/2 over TCP: by ALPN on a secure endpoint, or by the connection preface (h2c with prior
    /// knowledge) on a plaintext one. The <c>Upgrade:</c> dance is not implemented, which is what
    /// every deployed h2c client does.
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
    /// HTTP/1.1 over TCP and HTTP/3 over UDP, skipping HTTP/2 entirely.
    /// </summary>
    /// <remarks>
    /// Every client can still be served: one that speaks neither HTTP/2 nor HTTP/3 gets HTTP/1.1,
    /// and a browser told about the QUIC port by an Alt-Svc header moves itself there. Worth having
    /// when HTTP/2 is not wanted - its flow control and HPACK state cost per connection, and a
    /// deployment that has HTTP/3 may have little use for it.
    /// </remarks>
    Http1AndHttp3 = Http1 | Http3,

    /// <summary>
    /// HTTP/2 over TCP and HTTP/3 over UDP, with no HTTP/1.1 at all.
    /// </summary>
    /// <remarks>
    /// A client that speaks neither is turned away, so this is for somewhere the clients are known -
    /// a private API, or gRPC, where HTTP/2 is the floor rather than an upgrade. Note that a browser
    /// reaching a plaintext endpoint cannot use h2c, so this pairs with a certificate in practice.
    /// </remarks>
    Http2AndHttp3 = Http2 | Http3,

    /// <summary>Everything: HTTP/1.1 and HTTP/2 over TCP, HTTP/3 over UDP, one port number.</summary>
    All = Http1 | Http2 | Http3,
}
