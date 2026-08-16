namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// Protocol and TLS options for the ioxide engine.
/// </summary>
/// <remarks>
/// The port, its certificate and whether it asks for a client certificate stay on <c>Bind</c>,
/// where GenHTTP already puts them. Which protocols that port then serves lives here, because
/// GenHTTP's endpoint model has nowhere to put it.
/// </remarks>
public sealed record IoxideOptions
{
    internal static readonly IoxideOptions Default = new();

    /// <summary>
    /// The protocols every endpoint serves, unless <see cref="ProtocolsByPort"/> says otherwise.
    /// </summary>
    /// <remarks>
    /// Defaults to HTTP/1.1 alone. <c>Http1AndHttp2</c> lets the two share a port - ALPN picks on a
    /// secure endpoint, the connection preface on a plaintext one - and adding <c>Http3</c> binds
    /// the same port number over UDP as well.
    ///
    /// <para>An endpoint bound with <c>enableQuic</c> serves HTTP/3 whatever is set here, so code
    /// already using that flag keeps working.</para>
    /// </remarks>
    public IoxideProtocols Protocols { get; init; } = IoxideProtocols.Http1;

    /// <summary>
    /// Protocols for one port, overriding <see cref="Protocols"/>.
    /// </summary>
    /// <remarks>
    /// This is how endpoints get different protocols: bind the ports, then name the ones that differ.
    ///
    /// <code>
    /// .Bind(IPAddress.Any, 8080)                     // HTTP/1.1
    /// .Bind(IPAddress.Any, 8081)                     // HTTP/2 only, h2c
    /// .Bind(IPAddress.Any, 8443, certificate)        // all three
    ///
    /// new IoxideOptions
    /// {
    ///     ProtocolsByPort =
    ///     {
    ///         [8081] = IoxideProtocols.Http2,
    ///         [8443] = IoxideProtocols.All,
    ///     }
    /// }
    /// </code>
    ///
    /// <para>A port left out follows <see cref="Protocols"/>. A port given neither HTTP/1.1 nor
    /// HTTP/2 still has a TCP listener, since the endpoint is bound, so HTTP/1.1 is served there
    /// rather than accepting connections and answering nothing.</para>
    /// </remarks>
    public Dictionary<ushort, IoxideProtocols> ProtocolsByPort { get; init; } = [];

    /// <summary>
    /// PEM certificate chain for the HTTP/3 listener, as a path.
    /// </summary>
    /// <remarks>
    /// QUIC is terminated by ngtcp2, which loads PEM from disk rather than taking a certificate
    /// object. Setting this and <see cref="Http3KeyPath"/> hands it the files directly.
    ///
    /// <para>Left null, the certificate bound to the endpoint is exported to a temporary file
    /// instead - readable only by this user, and deleted on shutdown. That works, but it puts a
    /// private key on disk for the lifetime of the process, so a deployment that already has PEM
    /// files should name them here.</para>
    /// </remarks>
    public string? Http3CertificatePath { get; init; }

    /// <summary>PEM private key for the HTTP/3 listener. Pairs with <see cref="Http3CertificatePath"/>.</summary>
    public string? Http3KeyPath { get; init; }

    /// <summary>
    /// PEM bundle of trust anchors that client certificates are validated against, as a path.
    /// </summary>
    /// <remarks>
    /// Mutual TLS is enforced where the connection is terminated - by OpenSSL for HTTP/1.1 and
    /// HTTP/2, by ngtcp2 for HTTP/3 - so a chain that does not validate is refused before any
    /// request exists. An endpoint bound with a <c>certificateValidator</c> asks for a certificate;
    /// this is what the offered one is checked against.
    ///
    /// <para>The file's subject names are also sent in the CertificateRequest, so a client holding
    /// several certificates can pick the one this server accepts rather than guessing.
    /// <see cref="ClientCaPem"/> is trusted identically but sends no such hint.</para>
    /// </remarks>
    public string? ClientCaPath { get; init; }

    /// <summary>The client trust anchors as PEM text - the in-memory alternative to <see cref="ClientCaPath"/>.</summary>
    public string? ClientCaPem { get; init; }

    /// <summary>
    /// Refuse a client that offers no certificate at all.
    /// </summary>
    /// <remarks>
    /// False asks for one and validates what arrives, but lets a client offering nothing through -
    /// which is what a server serving both a public and a mutually authenticated route wants, since
    /// it can read who connected and decide per request. True is the usual choice for a private API.
    ///
    /// <para>An endpoint's <c>certificateValidator</c> can raise this on its own through
    /// <c>RequireCertificate</c>; the two are ORed.</para>
    /// </remarks>
    public bool RequireClientCertificate { get; init; }

    /// <summary>
    /// Bytes of QPACK dynamic table advertised to HTTP/3 clients.
    /// </summary>
    /// <remarks>
    /// 0 keeps every header literal against the static table, which costs bytes but can never stall
    /// a stream waiting for a table update. Only browsers advertise a table of their own; every
    /// other client measured sends 0, which makes the mechanism inert whatever is set here.
    /// </remarks>
    public long QpackDynamicTableCapacity { get; init; }

    /// <summary>
    /// How many HTTP/3 streams may wait on a QPACK table insertion. Only meaningful alongside a
    /// nonzero <see cref="QpackDynamicTableCapacity"/>, and the price paid for one.
    /// </summary>
    public long QpackBlockedStreams { get; init; }
}
