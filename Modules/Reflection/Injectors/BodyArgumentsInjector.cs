using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Reflection.Injectors;

public class BodyArgumentsInjector : IParameterInjector
{

    public bool Supports(IServer server, Type type) => type == typeof(BodyArguments);

    public async ValueTask<object?> GetValueAsync(IHandler handler, IRequest request, Type targetType)
    {
        var body = request.GetBody();

        return (body is null) ? BodyArguments.Empty : await body.AsBodyArgumentsAsync();
    }

}
