using ioxide;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>
/// The TCP listener that carries HTTP/1.1 and HTTP/2, alongside the QUIC one.
/// </summary>
public sealed partial class Server
{
    /// <summary>
    /// The ports that want a TCP listener: those serving HTTP/1.1 or HTTP/2, primary first - it
    /// becomes ioxide's <c>TcpOptions.Port</c> and the rest its <c>ExtraPorts</c>. Empty means this
    /// server serves HTTP/3 only and opens no TCP listener at all.
    /// </summary>
    /// <remarks>
    /// One listener bound to several ports rather than one per port: a connection carries the port
    /// it arrived on, which is what lets a single handler serve endpoints speaking different
    /// protocols.
    /// </remarks>
    private ushort[] ResolveTcpPorts()
        => _protocols.Where(p => (p.Value & Protocols.Http1AndHttp2) != 0)
                     .Select(p => p.Key)
                     .OrderBy(p => p == _primary.Port ? 0 : 1)
                     .ToArray();

    /// <summary>
    /// Adds the TCP listener for the ports resolved into <c>_tcpPorts</c>. Only called when
    /// there are any - an HTTP/3-only server gets a UDP socket and no TCP listener at all.
    /// </summary>
    /// <remarks>
    /// The tuning comes from the options; the ports come from the bindings and are not the
    /// caller's to set.
    /// </remarks>
    private ServerConfig WithTcp(ServerConfig serverConfig) => serverConfig with
    {
        // ioxide's, not ours - the engine's own TcpTransportOptions is what feeds it below.
        Tcp = new TcpOptions
        {
            Port = _tcpPorts[0],
            ExtraPorts = _tcpPorts[1..],

            ListenBacklog = _options.Tcp.ListenBacklog,
            WriteSlabSize = _options.Tcp.WriteSlabSize,
            WriteOverflow = _options.Tcp.WriteOverflow,
            PoolMax = _options.Tcp.PoolMax,
            ZeroCopySend = _options.Tcp.ZeroCopySend,
            RecvQueueEntries = _options.Tcp.RecvQueueEntries,
        },
    };
}
