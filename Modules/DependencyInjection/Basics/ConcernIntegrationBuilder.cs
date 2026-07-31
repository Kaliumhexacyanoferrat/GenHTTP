using GenHTTP.Api.Content;

namespace GenHTTP.Modules.DependencyInjection.Basics;

internal sealed class ConcernIntegrationBuilder<T> : IConcernBuilder where T : class, IDependentConcern
{

    public IConcern Build(IHandler content) => new ConcernIntegration<T>(content);

}
