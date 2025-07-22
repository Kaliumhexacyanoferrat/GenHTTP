using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.DependencyInjection.Internal;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Modules.DependencyInjection;

public static class Hosting
{

    public static IServerHost AddDependencyInjection(this IServerHost host, IServiceProvider services) => host.Add(new InjectionConcernBuilder(services));

}
