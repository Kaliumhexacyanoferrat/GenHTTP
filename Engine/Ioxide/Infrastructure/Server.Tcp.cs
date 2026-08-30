using ioxide;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>
/// The TCP listener that carries HTTP/1.1 and HTTP/2, alongside the QUIC one.
/// </summary>
public sealed partial class Server
{
    private readonly ushort[] _tcpPorts;
    
    /// <summary>
    /// The ports serving HTTP/1.1 or HTTP/2, primary first. One listener bound to several ports:
    /// a connection carries the port it arrived on, which is what lets one handler serve endpoints
    /// speaking different protocols. Empty on an HTTP/3-only server.
    /// </summary>
    private ushort[] ResolveTcpPorts()
        => _endPoints.Where(e => (e.Protocols & Protocols.Http1AndHttp2) != 0)
                     .Select(e => e.Port)
                     .OrderBy(p => p == _endPoints[0].Port ? 0 : 1)
                     .ToArray();

    /// <summary>
    /// Adds the TCP listener. The tuning comes from the options; the ports come from the bindings.
    /// </summary>
    private ServerConfig WithTcp(ServerConfig serverConfig) => serverConfig with
    {
        Tcp = new TcpOptions
        {
            Port = _tcpPorts[0],
            ExtraPorts = _tcpPorts[1..],

            ListenBacklog = _engineOptions.Tcp.ListenBacklog,
            WriteSlabSize = _engineOptions.Tcp.WriteSlabSize,
            WriteOverflow = _engineOptions.Tcp.WriteOverflow,
            PoolMax = _engineOptions.Tcp.PoolMax,
            ZeroCopySend = _engineOptions.Tcp.ZeroCopySend,
            RecvQueueEntries = _engineOptions.Tcp.RecvQueueEntries,
        },
    };
}
