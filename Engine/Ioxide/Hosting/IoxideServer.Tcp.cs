using ioxide;

namespace GenHTTP.Engine.Ioxide.Hosting;

/// <summary>
/// The TCP listener that carries HTTP/1.1 and HTTP/2, alongside the QUIC one.
/// </summary>
public sealed partial class IoxideServer
{
    /// <summary>
    /// Adds the TCP listener for the ports resolved into <c>_tcpRequested</c>. Only called when
    /// there are any - an HTTP/3-only server gets a UDP socket and no TCP listener at all.
    /// </summary>
    /// <remarks>
    /// One listener bound to several ports rather than one per port: a connection carries the port
    /// it arrived on, which is what lets a single handler serve endpoints speaking different
    /// protocols. The tuning comes from the options; the ports come from the bindings and are not
    /// the caller's to set.
    /// </remarks>
    private ServerConfig WithTcp(ServerConfig serverConfig) => serverConfig with
    {
        Tcp = new TcpOptions
        {
            Port = _tcpRequested[0],
            ExtraPorts = _tcpRequested[1..],

            ListenBacklog = _options.Tcp.ListenBacklog,
            WriteSlabSize = _options.Tcp.WriteSlabSize,
            WriteOverflow = _options.Tcp.WriteOverflow,
            PoolMax = _options.Tcp.PoolMax,
            ZeroCopySend = _options.Tcp.ZeroCopySend,
            RecvQueueEntries = _options.Tcp.RecvQueueEntries,
        },
    };
}
