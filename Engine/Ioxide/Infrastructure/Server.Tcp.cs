using ioxide;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

public sealed partial class Server
{
    private readonly ushort[] _tcpPorts;
    
    private ushort[] ResolveTcpPorts()
        => _endPoints.Where(e => (e.Protocols & Protocols.Http1AndHttp2) != 0)
                     .Select(e => e.Port)
                     .OrderBy(p => p == _endPoints[0].Port ? 0 : 1)
                     .ToArray();

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
