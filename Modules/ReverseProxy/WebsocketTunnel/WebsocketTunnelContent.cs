using System.IO.Pipelines;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ReverseProxy.WebsocketTunnel;

public class WebsocketTunnelContent : IResponseContent
{
    public ulong? Length => null!;

    private readonly IDuplexPipe _upstreamPipe;

    public WebsocketTunnelContent(IDuplexPipe upstreamPipe)
    {
        _upstreamPipe  = upstreamPipe;
    }

    public ValueTask<ulong?> CalculateChecksumAsync() => ValueTask.FromResult<ulong?>(null);

    public ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        // Manage the tunnel lifetime and logic

        return ValueTask.CompletedTask;
    }
}