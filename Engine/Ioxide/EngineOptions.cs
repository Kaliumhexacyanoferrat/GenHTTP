using ioxide;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// Tuning for the ioxide engine. Ports, certificates and mutual TLS stay on <c>Bind</c>.
/// </summary>
public sealed record EngineOptions
{
    internal static readonly EngineOptions Default = new();

    /// <summary>Protocols every endpoint serves unless <see cref="ProtocolsByPort"/> says otherwise.</summary>
    public Protocols Protocols { get; init; } = Protocols.Http1;

    /// <summary>Protocols for one port: <c>{ [8081] = Protocols.Http2, [8443] = Protocols.All }</c>.</summary>
    public Dictionary<ushort, Protocols> ProtocolsByPort { get; init; } = [];

    /// <summary>The reactors: how many, and the io_uring machinery each one owns.</summary>
    public ReactorOptions Reactor { get; init; } = new();

    /// <summary>The TCP transport, which carries HTTP/1.1 and HTTP/2.</summary>
    public TcpTransportOptions Tcp { get; init; } = new();

    /// <summary>The QUIC transport, which carries HTTP/3.</summary>
    public Http3Options Http3 { get; init; } = new();
}

/// <summary>
/// The reactors. Each owns a thread, a ring and its connections and shares nothing, so the memory
/// described here is multiplied by <see cref="ReactorCount"/>.
/// </summary>
public sealed record ReactorOptions
{
    /// <summary>How many reactors to run. Fewer than one per core where the box is shared.</summary>
    public int ReactorCount { get; init; } = Environment.ProcessorCount;

    /// <summary>io_uring submission and completion queue depth, per reactor.</summary>
    public uint RingEntries { get; init; } = 8192;

    /// <summary>Bytes per buffer in the shared recv ring. Unused when <see cref="Incremental"/> is set.</summary>
    public int RecvBufferSize { get; init; } = 32 * 1024;

    /// <summary>Buffers in the shared recv ring. Unused when <see cref="Incremental"/> is set.</summary>
    public int RecvSlots { get; init; } = 4096;

    /// <summary>
    /// Give each connection its own buffer ring (IOU_PBUF_RING_INC, kernel 6.12+) rather than
    /// sharing one. Setting this enables the mode and reserves the memory up front.
    /// </summary>
    public IncrementalOptions? Incremental { get; init; }
}

/// <summary>
/// The TCP transport, where OpenSSL terminates TLS for HTTP/1.1 and HTTP/2. None of this reaches
/// HTTP/3, which carries its own TLS 1.3 inside ngtcp2.
/// </summary>
public sealed record TcpTransportOptions
{
    /// <summary>
    /// Milliseconds a connection may take to finish its TLS handshake; 0 disables the sweep. Swept
    /// on the reactor tick (~250 ms), so a connection closes at the first tick past its deadline.
    /// </summary>
    public int HandshakeTimeoutMs { get; init; } = 10_000;

    /// <summary>
    /// TLS 1.3 ciphersuites, in OpenSSL's naming and preference order. Null keeps OpenSSL's
    /// defaults. Constrains 1.3 only - see <see cref="CipherList"/> - and is refused together with
    /// <see cref="TxKernelTls"/>, which needs TLS_AES_128_GCM_SHA256.
    /// </summary>
    public string? CipherSuites { get; init; }

    /// <summary>
    /// Ciphers for TLS 1.2 and below, in OpenSSL's cipher-list syntax. Null keeps OpenSSL's
    /// defaults. No effect on a 1.3 handshake.
    /// </summary>
    public string? CipherList { get; init; }

    /// <summary>
    /// Produce TLS records in the kernel (kTLS) on send instead of in OpenSSL, which still drives
    /// the handshake. Requires the Linux <c>tls</c> module and TLS 1.3.
    /// </summary>
    public bool TxKernelTls { get; init; }

    /// <summary>
    /// Decrypt in the kernel on receive. Experimental, requires <see cref="TxKernelTls"/>, and the
    /// peer must send no post-handshake control records.
    /// </summary>
    public bool RxKernelTls { get; init; }

    /// <summary>listen() backlog per reactor - each binds its own SO_REUSEPORT listener.</summary>
    public int ListenBacklog { get; init; } = 1024;

    /// <summary>Bytes of write buffer per connection. A larger response takes <see cref="WriteOverflow"/>.</summary>
    public int WriteSlabSize { get; init; } = 16 * 1024;

    /// <summary>Grow the slab, or chain pooled slabs and flush them with one vectored sendmsg.</summary>
    public WriteOverflowStrategy WriteOverflow { get; init; } = WriteOverflowStrategy.Grow;

    /// <summary>Connections kept pooled per reactor for reuse rather than freed.</summary>
    public int PoolMax { get; init; } = 1024;

    /// <summary>
    /// Send with IORING_OP_SEND_ZC: no in-kernel copy, but pinned pages and a second completion, so
    /// it only pays for large responses. kTLS connections fall back to a plain send.
    /// </summary>
    public bool ZeroCopySend { get; init; }

    /// <summary>Depth of the per-connection recv queue, a power of two. Overflow closes it.</summary>
    public int RecvQueueEntries { get; init; } = 64;
}

/// <summary>
/// The QUIC transport: its listener, the UDP socket underneath and QPACK. Only consulted where a
/// port serves <see cref="Protocols.Http3"/>; the certificate comes off the binding.
/// </summary>
public sealed record Http3Options
{
    /// <summary>
    /// Milliseconds a connection may spend not finishing its QUIC handshake; 0 removes the bound.
    /// Not covered by <see cref="IdleTimeoutMs"/>, which any inbound datagram refreshes.
    /// </summary>
    public int HandshakeTimeoutMs { get; init; } = 10_000;

    /// <summary>
    /// Transport backstop, in milliseconds, for a connection whose engine went quiet; 0 disables
    /// the sweep. QUIC's own idle timeout is negotiated inside the connection.
    /// </summary>
    public int IdleTimeoutMs { get; init; } = 60_000;

    /// <summary>
    /// How a datagram reaches the reactor owning its connection once a client changes address.
    /// <see cref="QuicRouting.Forward"/> costs nothing until one migrates, then posts the datagram
    /// to its owner; <see cref="QuicRouting.KernelFilter"/> routes by connection id in a BPF program
    /// attached to the SO_REUSEPORT group, which everyone pays for at saturation.
    /// </summary>
    public QuicRouting Routing { get; init; } = QuicRouting.Forward;

    /// <summary>
    /// Under <see cref="QuicRouting.Forward"/>, claim a migrated client's new address so forwarding
    /// stops. Costs one descriptor per migrated connection.
    /// </summary>
    public bool PinMigratedPeers { get; init; } = true;

    /// <summary>
    /// Requested UDP socket buffer, in bytes - the kernel clamps it to <c>net.core.rmem_max</c> /
    /// <c>wmem_max</c> and ioxide logs the granted size. Worth measuring rather than maximising:
    /// granting the full 8 MiB cost ~45% of throughput at saturation on ioxide's own benchmark,
    /// because a deep standing queue replaced the early drops congestion control reads.
    /// </summary>
    public int SocketBufferBytes { get; init; } = 8 * 1024 * 1024;

    /// <summary>
    /// Bytes of QPACK dynamic table advertised to clients. 0 keeps every header literal, which
    /// costs bytes but can never stall a stream on a table update.
    /// </summary>
    public long QpackDynamicTableCapacity { get; init; }

    /// <summary>
    /// Streams that may wait on a QPACK table insertion - the price of a nonzero
    /// <see cref="QpackDynamicTableCapacity"/>, and meaningless without one.
    /// </summary>
    public long QpackBlockedStreams { get; init; }
}
