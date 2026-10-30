using ioxide;

namespace GenHTTP.Engine.Ioxide;

/// <summary>How the engine itself is tuned, beneath what the bindings decide.</summary>
public sealed record EngineOptions
{
    internal static readonly EngineOptions Default = new();

    public ReactorOptions Reactor { get; init; } = new();

    public TcpTransportOptions Tcp { get; init; } = new();

    public QuicTransportOptions Quic { get; init; } = new();

    public Http3Options Http3 { get; init; } = new();
}

/// <summary>The io_uring reactors: how many, and how much ring each one gets.</summary>
public sealed record ReactorOptions
{
    public int ReactorCount { get; init; } = Environment.ProcessorCount;

    public uint RingEntries { get; init; } = 8192;

    public int RecvBufferSize { get; init; } = 32 * 1024;

    public int RecvSlots { get; init; } = 4096;

    public IncrementalOptions? Incremental { get; init; }
}

/// <summary>The TCP listener and its TLS, from the listen backlog to the kTLS switches.</summary>
public sealed record TcpTransportOptions
{
    public int HandshakeTimeoutMs { get; init; } = 10_000;

    public string? CipherSuites { get; init; }

    public string? CipherList { get; init; }

    public bool TxKernelTls { get; init; }

    public bool RxKernelTls { get; init; }

    public int ListenBacklog { get; init; } = 1024;

    public int WriteSlabSize { get; init; } = 16 * 1024;

    public WriteOverflowStrategy WriteOverflow { get; init; } = WriteOverflowStrategy.Grow;

    public int PoolMax { get; init; } = 1024;

    public bool ZeroCopySend { get; init; }

    public int RecvQueueEntries { get; init; } = 64;

    /// <summary>
    /// Close a connection that has waited this long on its client: a read parked, nothing
    /// buffered and nothing being sent. 0 (the default) disables it.
    /// </summary>
    /// <remarks>
    /// The clock only runs while the server waits on the client. Bytes in either direction
    /// restart it, a send in flight holds it, and the time a handler spends producing a response
    /// does not count - so a slow handler keeps its connection, while an idle keep-alive
    /// connection or a silent websocket does not. HTTP/2 is the exception: its frame loop keeps a
    /// read parked while handlers work, so a response slower than this, to a client that sends
    /// nothing in the meantime, is cut.
    ///
    /// Off by default because this listener also carries upgraded connections, and an idle
    /// websocket is a healthy one. ioxide's own default is 60s, which is right for HTTP keep-alive
    /// and wrong for a websocket whose ping interval is longer than that - the transport cannot
    /// tell the two apart, so the framework does not guess on your behalf.
    ///
    /// Turn it on if you serve no upgrades, or set it comfortably above your websocket ping
    /// interval - a ping from either side restarts the clock.
    /// </remarks>
    public int ReadTimeoutMs { get; init; }

    [Obsolete("Please use 'ReadTimeoutMs' instead")]
    public int IdleTimeoutMs { get => ReadTimeoutMs; init => ReadTimeoutMs = value; }

    /// <summary>
    /// Close a connection whose send has been in flight for this long. 0 (the default) disables it.
    /// </summary>
    /// <remarks>
    /// This is what bounds a client that stops reading: its window shuts, the send never completes,
    /// and without a clock the connection is pinned for as long as the peer cares to hold it. TCP
    /// will not end it on its own, and neither will <see cref="ReadTimeoutMs"/>: a client that
    /// keeps sending restarts that clock while its own window stays shut. A websocket written from
    /// a background task is the case that needs this one.
    ///
    /// Off by default because it is a deadline for a whole flush, not for progress within it, and
    /// content that does not flush as it writes goes out as one flush - so a large download to a
    /// slow client counts against it. Size it against your slowest legitimate response rather than
    /// against a stall - for an API that serves small payloads, something like 30s is a
    /// straightforward win.
    /// </remarks>
    public int SendTimeoutMs { get; init; }
}

/// <summary>The QUIC transport: its timeouts, how datagrams are routed, and the UDP socket.</summary>
public sealed record QuicTransportOptions
{
    public int HandshakeTimeoutMs { get; init; } = 10_000;

    /// <summary>
    /// Close a connection whose client has sent nothing for this long. 0 disables it.
    /// </summary>
    /// <remarks>
    /// While a response is still owed, the engine keeps the client answering with keep-alive
    /// PINGs, so a slow handler is not timed out - only a client that went quiet with nothing
    /// pending is.
    /// </remarks>
    public int ReadTimeoutMs { get; init; } = 60_000;

    [Obsolete("Please use 'ReadTimeoutMs' instead")]
    public int IdleTimeoutMs { get => ReadTimeoutMs; init => ReadTimeoutMs = value; }

    public QuicRouting Routing { get; init; } = QuicRouting.Forward;

    public bool PinMigratedPeers { get; init; } = true;

    public int SocketBufferBytes { get; init; } = 8 * 1024 * 1024;
}

/// <summary>HTTP/3 above QUIC, which today means QPACK alone.</summary>
public sealed record Http3Options
{
    public long QpackDynamicTableCapacity { get; init; }

    public long QpackBlockedStreams { get; init; }
}
