namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// Protocol and TLS options for the ioxide engine. The port, its certificate and whether it asks
/// for a client certificate stay on <c>Bind</c>; which protocols it then serves lives here.
/// </summary>
public sealed record IoxideOptions
{
    internal static readonly IoxideOptions Default = new();

    /// <summary>
    /// The protocols every endpoint serves, unless <see cref="ProtocolsByPort"/> says otherwise.
    /// An endpoint bound with <c>enableQuic</c> serves HTTP/3 whatever is set here.
    /// </summary>
    public IoxideProtocols Protocols { get; init; } = IoxideProtocols.Http1;

    /// <summary>
    /// Protocols for one port, overriding <see cref="Protocols"/> - bind the ports, then name the
    /// ones that differ: <c>{ [8081] = IoxideProtocols.Http2, [8443] = IoxideProtocols.All }</c>.
    /// </summary>
    public Dictionary<ushort, IoxideProtocols> ProtocolsByPort { get; init; } = [];

    /// <summary>The HTTP/3 endpoint: the certificate QUIC serves, and QPACK.</summary>
    public IoxideHttp3Options Http3 { get; init; } = new();

    /// <summary>Client certificates: what they are validated against, and whether one is required.</summary>
    public IoxideMutualTlsOptions MutualTls { get; init; } = new();
}

/// <summary>
/// The HTTP/3 endpoint. Only consulted when a port serves <see cref="IoxideProtocols.Http3"/>.
/// </summary>
public sealed record IoxideHttp3Options
{
    /// <summary>
    /// PEM certificate chain for the HTTP/3 listener, as a path. Required to serve HTTP/3, and
    /// should be the certificate bound to the endpoint rather than a second one - it is named
    /// separately only because ngtcp2 loads PEM by path, and the engine writes none for you.
    /// </summary>
    public string? CertificatePath { get; init; }

    /// <summary>PEM private key for the HTTP/3 listener. Pairs with <see cref="CertificatePath"/>.</summary>
    public string? KeyPath { get; init; }

    /// <summary>
    /// Bytes of QPACK dynamic table advertised to HTTP/3 clients. 0 keeps every header literal
    /// against the static table, which costs bytes but can never stall a stream on a table update.
    /// </summary>
    public long QpackDynamicTableCapacity { get; init; }

    /// <summary>
    /// How many HTTP/3 streams may wait on a QPACK table insertion. Only meaningful alongside a
    /// nonzero <see cref="QpackDynamicTableCapacity"/>, and the price paid for one.
    /// </summary>
    public long QpackBlockedStreams { get; init; }
}

/// <summary>
/// What client certificates are validated against - by OpenSSL for HTTP/1.1 and HTTP/2, by ngtcp2
/// for HTTP/3, so a bad chain is refused before any request exists. WHICH endpoints ask for one is
/// decided per endpoint, by the <c>certificateValidator</c> passed to <c>Bind</c>.
/// </summary>
public sealed record IoxideMutualTlsOptions
{
    /// <summary>
    /// PEM bundle of trust anchors that client certificates are validated against, as a path. Its
    /// subject names are also sent in the CertificateRequest, so a client holding several
    /// certificates can pick the right one; <see cref="ClientCaPem"/> sends no such hint.
    /// </summary>
    public string? ClientCaPath { get; init; }

    /// <summary>The trust anchors as PEM text - the in-memory alternative to <see cref="ClientCaPath"/>.</summary>
    public string? ClientCaPem { get; init; }

    /// <summary>
    /// Refuse a client that offers no certificate, on every secure endpoint; false still asks for
    /// one and validates what arrives. Usually left alone, since an endpoint's
    /// <c>certificateValidator</c> raises it for that endpoint. The two are ORed.
    /// </summary>
    public bool RequireClientCertificate { get; init; }
}
