using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.DependencyInjection.Internal;

public class DependencyInjector : IParameterInjector
{

    public bool Supports(Type type) => throw new NotImplementedException(); // ToDo: need the request here to tell

    public object? GetValue(IHandler handler, IRequest request, Type targetType) => throw new NotImplementedException();

}
