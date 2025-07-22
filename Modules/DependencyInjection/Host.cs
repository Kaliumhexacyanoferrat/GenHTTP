using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.DependencyInjection.Internal;

namespace GenHTTP.Modules.DependencyInjection;

public static class Host
{

    public static IServerHost AddDependencyInjection(this IServerHost host, IServiceProvider services) => host.Add(new InjectionConcernBuilder(services));

}
