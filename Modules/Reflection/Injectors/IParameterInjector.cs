using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Injectors;

public interface IParameterInjector
{

    bool Supports(IServer server, Type type);

    ValueTask<object?> GetValueAsync(IHandler handler, IRequest request, Type targetType);

}
