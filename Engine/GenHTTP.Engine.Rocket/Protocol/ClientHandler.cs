using GenHTTP.Engine.Rocket.Infrastructure;
using Microsoft.Extensions.ObjectPool;

namespace GenHTTP.Engine.Rocket.Protocol;

internal sealed class ClientHandler
{
    private static readonly DefaultObjectPool<ClientContext> ContextPool = new(new ClientContextPolicy(), 1024 * 64);
    
    private readonly RocketServer _server;
    
    public ClientHandler(RocketServer server)
    {
        _server = server;
    }
    
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            while (_server.Engine.ServerRunning) 
            {
                var conn = await _server.Engine.AcceptAsync(cancellationToken);
                
                if(conn is null)
                    continue;
                
                Console.WriteLine($"Connection: {conn.ClientFd}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Signaled to stop");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Cowabunga!] {e.Message}");
        }
        finally
        {
            _server.Engine.Stop();
        }
    }
}