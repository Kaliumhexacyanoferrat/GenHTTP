using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Injectors;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Modules.DependencyInjection.Framework;

public class DependencyInjector : IParameterInjector
{

    public bool Supports(IRequest request, Type type)
    {
        var scope = request.GetServiceScope();

        var supportService = scope.ServiceProvider.GetService<IServiceProviderIsService>();

        if (supportService != null)
        {
            return supportService.IsService(type);
        }

        return (scope.ServiceProvider.GetService(type) != null);
    }

    public object? GetValue(IHandler handler, IRequest request, Type targetType)
    {
        var scope = request.GetServiceScope();

        return scope.ServiceProvider.GetService(targetType);
    }

}
