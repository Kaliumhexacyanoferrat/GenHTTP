using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection;

public class MethodCollectionFactory
{
    private readonly Func<IRequest, Task<MethodCollection>> _factory;

    private volatile Task<MethodCollection>? _instance;

    public MethodCollectionFactory(Func<IRequest, Task<MethodCollection>> factory)
    {
        _factory = factory;
    }

    public Task<MethodCollection> GetAsync(IRequest request)
    {
        var existing = _instance;
        
        if (existing != null)
        {
            return existing;
        }

        var task = _factory(request);
        
        var original = Interlocked.CompareExchange(ref _instance, task, null);
        
        return original ?? task;
    }
    
}
