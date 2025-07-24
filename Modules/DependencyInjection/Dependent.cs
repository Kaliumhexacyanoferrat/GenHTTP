using GenHTTP.Api.Content;

using GenHTTP.Modules.DependencyInjection.Basics;

namespace GenHTTP.Modules.DependencyInjection;

public static class Dependent
{

    public static IConcernBuilder Concern<T>() where T : class, IDependentConcern => new ConcernIntegrationBuilder<T>();

    public static IHandlerBuilder Handler<T>() where T : class, IDependentHandler => new HandlerIntegrationBuilder<T>();

}
