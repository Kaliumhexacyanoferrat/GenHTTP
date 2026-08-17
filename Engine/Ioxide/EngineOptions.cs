using ioxide;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// Protocol and TLS options for the ioxide engine. The port, its certificate and whether it asks
/// for a client certificate stay on <c>Bind</c>; which protocols it then serves lives here.
/// </summary>
public sealed record EngineOptions
{
    internal static readonly EngineOptions Default = new();

    /// <summary>
    /// The protocols every endpoint serves, unless <see cref="ProtocolsByPort"/> says otherwise.
    /// An endpoint bound with <c>enableQuic</c> serves HTTP/3 whatever is set here.
    /// </summary>
    public Protocols Protocols { get; init; } = Protocols.Http1;

    /// <summary>
    /// Protocols for one port, overriding <see cref="Protocols"/> - bind the ports, then name the
    /// ones that differ: <c>{ [8081] = Protocols.Http2, [8443] = Protocols.All }</c>.
    /// </summary>
    public Dictionary<ushort, Protocols> ProtocolsByPort { get; init; } = [];

    /// <summary>The reactors: how many, and the io_uring machinery each one owns.</summary>
    public ReactorOptions Reactor { get; init; } = new();

    /// <summary>The TCP endpoints: how TLS is terminated for HTTP/1.1 and HTTP/2.</summary>
    public TcpTransportOptions Tcp { get; init; } = new();

    /// <summary>The HTTP/3 endpoint: the certificate QUIC serves, and QPACK.</summary>
    public Http3Options Http3 { get; init; } = new();
}

/// <summary>
/// The reactors. Each runs on its own thread, owns an io_uring ring and the connections accepted
/// on it, and shares nothing with the others - so these are per reactor, not per server, and the
/// memory they describe is multiplied by <see cref="ReactorCount"/>.
/// </summary>
public sealed record ReactorOptions
{
    /// <summary>
    /// How many reactors to run. One per core suits a server with the machine to itself; anything
    /// sharing the box - a colocated load generator, a database, sibling containers - wants fewer,
    /// or the reactors and everything else fight for the same cores.
    /// </summary>
    public int ReactorCount { get; init; } = Environment.ProcessorCount;

    /// <summary>io_uring submission and completion queue depth, per reactor.</summary>
    public uint RingEntries { get; init; } = 8192;

    /// <summary>
    /// Bytes per buffer in the shared recv ring. Larger reads more per completion and wastes more
    /// per idle connection. Unused when <see cref="Incremental"/> is set.
    /// </summary>
    public int RecvBufferSize { get; init; } = 32 * 1024;

    /// <summary>
    /// Buffers in the shared recv ring. Running out costs a retry, not a lost byte. Unused when
    /// <see cref="Incremental"/> is set.
    /// </summary>
    public int RecvSlots { get; init; } = 4096;

    /// <summary>
    /// Give each connection its own small buffer ring (IOU_PBUF_RING_INC, kernel 6.12+) instead of
    /// drawing from the shared one. Setting this IS enabling the mode, and the two shared-ring
    /// knobs above then go unused. Reserves MaxConnections x RecvSlots x RecvBufferSize per
    /// reactor up front, so it trades memory for not sharing a ring between connections.
    /// </summary>
    public IncrementalOptions? Incremental { get; init; }
}

/// <summary>
/// The TCP endpoints, where OpenSSL terminates TLS for HTTP/1.1 and HTTP/2. HTTP/3 is not
/// configured here: QUIC carries its own TLS 1.3 inside ngtcp2, so none of this reaches it.
/// </summary>
public sealed record TcpTransportOptions
{
    /// <summary>
    /// Produce TLS records in the kernel (kTLS) on the send path instead of in OpenSSL, which
    /// still drives the handshake. Requires the Linux <c>tls</c> module and TLS 1.3.
    /// </summary>
    public bool TxKernelTls { get; init; }

    /// <summary>
    /// Decrypt TLS records in the kernel on the receive path. Experimental, and requires
    /// <see cref="TxKernelTls"/> - RX shares the ULP handoff TX installs, so ioxide refuses RX
    /// alone. The peer must send no post-handshake control records.
    /// </summary>
    public bool RxKernelTls { get; init; }

    /// <summary>
    /// listen() backlog per reactor - the accept queue that absorbs a burst of connections. Every
    /// reactor binds its own SO_REUSEPORT listener, so the server absorbs this many per reactor.
    /// </summary>
    public int ListenBacklog { get; init; } = 1024;

    /// <summary>
    /// Bytes of write buffer per connection. A response that fits leaves in one send; a larger one
    /// is handled by <see cref="WriteOverflow"/>.
    /// </summary>
    public int WriteSlabSize { get; init; } = 16 * 1024;

    /// <summary>
    /// What a response larger than <see cref="WriteSlabSize"/> does: grow the slab and keep one
    /// send, or chain pooled slabs and flush them with one vectored sendmsg instead of reallocating.
    /// </summary>
    public WriteOverflowStrategy WriteOverflow { get; init; } = WriteOverflowStrategy.Grow;

    /// <summary>Connections kept pooled per reactor for reuse rather than freed.</summary>
    public int PoolMax { get; init; } = 1024;

    /// <summary>
    /// Send responses with zero-copy (IORING_OP_SEND_ZC) instead of a normal send. Trades the
    /// in-kernel payload copy for page pinning and a second completion per send, so it only pays
    /// for large responses. kTLS connections always fall back to a plain send.
    /// </summary>
    public bool ZeroCopySend { get; init; }

    /// <summary>
    /// Depth of the per-connection recv queue, a power of two. Overflow closes the connection.
    /// </summary>
    public int RecvQueueEntries { get; init; } = 64;
}

/// <summary>
/// The HTTP/3 endpoint. Only consulted when a port serves <see cref="Protocols.Http3"/>.
/// </summary>
public sealed record Http3Options
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
