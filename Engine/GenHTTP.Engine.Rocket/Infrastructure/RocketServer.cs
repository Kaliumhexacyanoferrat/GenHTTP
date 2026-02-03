using System.Reflection;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Infrastructure;
using URocket.Engine.Configs;

namespace GenHTTP.Engine.Rocket.Infrastructure;

internal sealed class RocketServer : IServer
{
    public string Version { get; }
    
    public bool Running { get; }

    public bool Development => Configuration.DevelopmentMode;
    
    public IEndPointCollection EndPoints { get; } = null!;
    
    public IServerCompanion? Companion { get; }
    
    public IHandler Handler { get; }

    internal URocket.Engine.Engine Engine { get; }
    
    internal ServerConfiguration Configuration { get; }

    internal RocketServer(IServerCompanion? companion, IHandler handler, ServerConfiguration config, EngineOptions engineOptions)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "(n/a)";
        
        Companion = companion;
        Handler = handler;
        
        Configuration = config;
        
        Engine = new URocket.Engine.Engine(engineOptions);

        Running = Engine.ServerRunning;
    }
    
    private static async ValueTask PrepareHandlerAsync(IHandler handler, IServerCompanion? companion)
    {
        try
        {
            await handler.PrepareAsync();
        }
        catch (Exception e)
        {
            companion?.OnServerError(ServerErrorScope.General, null, e);
        }
    }
    
    public async ValueTask StartAsync()
    {
        await PrepareHandlerAsync(Handler, Companion);
        
        Engine.Listen();
    }
    
    public ValueTask DisposeAsync()
    {
        Engine.Stop();
        
        return ValueTask.CompletedTask;
    }
}