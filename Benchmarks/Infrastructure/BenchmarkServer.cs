using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Benchmarks.Infrastructure;

public class BenchmarkServer(IHandler handler) : IServer
{
    private bool _running = true;

    public string Version => "0.1";

    public bool Running => _running;

    public bool Development => false;

    public IEndPointCollection EndPoints => throw new NotSupportedException(); 

    public IServerCompanion? Companion => null;

    public IHandler Handler => handler;

    public ValueTask StartAsync()
    {
        _running = true;
        return default;
    }
    
    public ValueTask DisposeAsync()
    {
        _running = false;
        return default;
    }
    
}
