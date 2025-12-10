using System.IO.Pipelines;

namespace GenHTTP.Modules.ReverseProxy.Websocket;

public sealed class DuplexPipe(PipeReader input, PipeWriter output) : IDuplexPipe, IAsyncDisposable
{
    public PipeReader Input { get; } = input;
    public PipeWriter Output { get; } = output;

    public async ValueTask DisposeAsync()
    {
        await Input.CompleteAsync();
        await Output.CompleteAsync();
    }
    
}