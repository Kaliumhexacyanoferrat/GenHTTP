using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Injectors;

public interface IParameterInjector
{

    bool Supports(IServer server, Type type);

    object? GetValue(IHandler handler, IRequest request, Type targetType);

}
