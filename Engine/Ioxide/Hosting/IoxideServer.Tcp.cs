using ioxide;

namespace GenHTTP.Engine.Ioxide.Hosting;

/// <summary>
/// The TCP listener that carries HTTP/1.1 and HTTP/2, alongside the QUIC one.
/// </summary>
public sealed partial class IoxideServer
{
    /// <summary>
    /// Adds the TCP listener, or none at all when no endpoint serves anything over TCP - an
    /// HTTP/3-only server gets a UDP socket and nothing else, rather than a listener that accepts
    /// connections it would answer with nothing.
    /// </summary>
    /// <remarks>
    /// One listener bound to several ports rather than one per port: the connection carries the
    /// port it arrived on, which is what lets a single handler serve endpoints with different
    /// protocols. The transport tuning comes from the options; the ports come from the bindings and
    /// are not the caller's to set here.
    /// </remarks>
    private ServerConfig WithTcp(ServerConfig serverConfig)
    {
        var tcpPorts = _protocols.Where(p => (p.Value & IoxideProtocols.Http1AndHttp2) != 0)
                                 .Select(p => p.Key)
                                 .OrderBy(p => p == _primary.Port ? 0 : 1)
                                 .ToArray();

        if (tcpPorts.Length == 0)
        {
            return serverConfig with { Tcp = null };
        }

        return serverConfig with
        {
            Tcp = new TcpOptions
            {
                Port = tcpPorts[0],
                ExtraPorts = tcpPorts.Skip(1).ToArray(),

                ListenBacklog = _options.Tcp.ListenBacklog,
                WriteSlabSize = _options.Tcp.WriteSlabSize,
                WriteOverflow = _options.Tcp.WriteOverflow,
                PoolMax = _options.Tcp.PoolMax,
                ZeroCopySend = _options.Tcp.ZeroCopySend,
                RecvQueueEntries = _options.Tcp.RecvQueueEntries,
            },
        };
    }
}
