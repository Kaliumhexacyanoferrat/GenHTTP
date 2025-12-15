using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Imperative;

public class ImperativeWebsocketBuilder : WebsocketBuilder<ImperativeWebsocketBuilder>
{
    private IImperativeHandler? _handler;

    /// <summary>
    /// Sets the handler used to establish a message pump.
    /// </summary>
    /// <param name="handler">The handler used to establish a message pump</param>
    public ImperativeWebsocketBuilder Handler(IImperativeHandler handler)
    {
        _handler = handler;
        return this;
    }

    public override IHandler Build()
    {
        if (_handler is null)
        {
            throw new BuilderMissingPropertyException("Handler");
        }

        var contentFactory = (IRequest r) => new ImperativeWebsocketContent(_handler, r, _maxRxBufferSize, _handleContinuationFramesManually);

        return Concerns.Chain(_concerns, new WebsocketHandler(contentFactory));
    }

}
