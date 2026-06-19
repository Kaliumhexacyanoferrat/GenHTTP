using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Injectors;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Modules.DependencyInjection.Infrastructure;

public class DependencyInjector : IParameterInjector
{

    public bool Supports(IServer server, Type type)
    {
        var provider = server.GetServiceProvider();

        var supportService = provider.GetService<IServiceProviderIsService>();

        if (supportService != null)
        {
            return supportService.IsService(type);
        }

        return (provider.GetService(type) != null);
    }

    public ValueTask<object?> GetValueAsync(IHandler handler, IRequest request, Type targetType)
    {
        var scope = request.GetServiceScope();

        return new ValueTask<object?>(scope.ServiceProvider.GetService(targetType));
    }

}
