using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Layouting.Provider;

internal sealed class HandlerWrapper(IHandler handler) : IHandlerBuilder
{

    public IHandler Build() => handler;

}
