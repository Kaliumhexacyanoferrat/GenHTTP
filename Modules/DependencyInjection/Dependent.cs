using GenHTTP.Api.Content;

using GenHTTP.Modules.DependencyInjection.Basics;

namespace GenHTTP.Modules.DependencyInjection;

/// <summary>
/// Allows to enable dependency injection for concerns and handlers.
/// </summary>
public static class Dependent
{

    /// <summary>
    /// Allows the given class to be used as a regular concern with dependency injection enabled.
    /// </summary>
    /// <typeparam name="T">The class implementing the concern</typeparam>
    public static IConcernBuilder Concern<T>() where T : class, IDependentConcern => new ConcernIntegrationBuilder<T>();

    /// <summary>
    /// Allows the given class to be used as a regular handler with dependency injection enabled.
    /// </summary>
    /// <typeparam name="T">The class implementing the handler</typeparam>
    public static IHandlerBuilder Handler<T>() where T : class, IDependentHandler => new HandlerIntegrationBuilder<T>();

}
