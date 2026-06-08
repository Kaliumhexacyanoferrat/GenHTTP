using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets.Provider;

public class WebsocketHandler(Func<IRequest, IResponseContent> contentFactory) : IHandler
{
    private static readonly ByteString KeyHeader = new("Sec-WebSocket-Key");

    private static readonly ByteString VersionHeader = new("Sec-WebSocket-Version");

    private static readonly ByteString AcceptHeader = new("Sec-WebSocket-Accept");

    private static readonly ByteString UpgradeHeaderName = new("Upgrade");

    private static readonly ByteString UpgradeHeaderValue = new("websocket");

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var key = ValidateRequest(request);

        var content = contentFactory(request);

        var response = request.Respond()
                              .Status(ResponseStatus.SwitchingProtocols)
                              .Connection(Connection.Upgrade)
                              .Header(UpgradeHeaderName, UpgradeHeaderValue)
                              .Header(AcceptHeader, Handshake.CreateAcceptKey(key))
                              .Content(content)
                              .Build();

        return ValueTask.FromResult<IResponse?>(response);
    }

    private static ByteString ValidateRequest(IRequest request)
    {
        var headers = request.Header.Headers;

        if (request.Header.Method != RequestMethod.Get)
        {
            throw new ProviderException(ResponseStatus.MethodNotAllowed, "Websocket connections can only be initiated via GET");
        }

        var key = headers.GetEntry(KeyHeader)
            ?? throw new ProviderException(ResponseStatus.BadRequest, "Client did not initiate websocket handshake. Header Sec-WebSocket-Key is missing from the request.");

        var version = headers.GetEntry(VersionHeader)
            ?? throw new ProviderException(ResponseStatus.BadGateway, "Client does not specify the requested websocket version.");

        if (!int.TryParse(version.Bytes.Span, out var versionInt) || versionInt != 13)
        {
            throw new ProviderException(ResponseStatus.UpgradeRequired, "Only version 13 is supported by this websocket server.", (b) => b.Header("Sec-WebSocket-Version", "13"));
        }

        return key;
    }

}
