using GenHTTP.Api.Infrastructure;
using ioxide;

namespace GenHTTP.Engine.Ioxide;

/// <summary>How the engine itself is tuned, beneath what the bindings decide.</summary>
public sealed record EngineOptions
{
    internal static readonly EngineOptions Default = new();

    public HttpProtocols HttpProtocols { get; init; } = HttpProtocols.Http1;

    public Dictionary<ushort, HttpProtocols> ProtocolsByPort { get; init; } = [];

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
}

/// <summary>The QUIC transport: its timeouts, how datagrams are routed, and the UDP socket.</summary>
public sealed record QuicTransportOptions
{
    public int HandshakeTimeoutMs { get; init; } = 10_000;

    public int IdleTimeoutMs { get; init; } = 60_000;

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
