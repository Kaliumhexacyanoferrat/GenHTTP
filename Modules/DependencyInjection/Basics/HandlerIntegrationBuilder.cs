using GenHTTP.Api.Content;

namespace GenHTTP.Modules.DependencyInjection.Basics;

internal sealed class HandlerIntegrationBuilder<T> : IHandlerBuilder where T : class, IDependentHandler
{

    public IHandler Build() => new HandlerIntegration<T>();

}
