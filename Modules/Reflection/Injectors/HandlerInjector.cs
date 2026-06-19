using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Injectors;

public class HandlerInjector : IParameterInjector
{

    public bool Supports(IServer server, Type type) => type == typeof(IHandler);

    public ValueTask<object?> GetValueAsync(IHandler handler, IRequest request, Type targetType) => new(handler);
    
}
