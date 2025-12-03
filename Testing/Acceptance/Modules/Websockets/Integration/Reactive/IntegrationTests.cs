using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration.Reactive;

[TestClass]
public sealed class IntegrationTests
{

    [TestMethod]
    public async Task TestServerReactive()
    {
        // todo: create another flavor "Functional" out of this

        /*var reactiveWebsocket = GenHTTP.Modules.Websockets.Websocket.CreateReactive(rxBufferSize: 1024)
                                       // Ping when connection is established, tests outgoing ping
                                       .OnConnected(async stream => await stream.PingAsync())
                                       // Test incoming pings, very rare, browsers do not ping
                                       .OnPing(async (stream, frame) => await stream.PongAsync(frame.Data))
                                       .OnPong(stream => ValueTask.CompletedTask)
                                       .OnMessage(async (stream, frame) => await stream.WriteAsync(frame.Data))
                                       // Also very rarely used feature
                                       .OnContinue(async (stream, frame) => await stream.WriteAsync(frame.Data))
                                       .OnClose(async (stream, frame) => await stream.CloseAsync())
                                       .OnError((stream, error) => new ValueTask<bool>(false));*/

        var websocket = GenHTTP.Modules.Websockets.Websocket.CreateReactive()
                               .MaxFrameSize(1024)
                               .Handler(new ReactiveHandler());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.Execute(host.Port);
    }
    
    [TestMethod]
    public async Task TestServerReactiveFragmented()
    {
        var websocket = GenHTTP.Modules.Websockets.Websocket.CreateReactive()
            .MaxFrameSize(1024)
            .Handler(new ReactiveHandlerFragmented());

        Chain.Works(websocket);

        await using var host = await TestHost.RunAsync(websocket);

        await Client.ExecuteFragmented("127.0.0.1", host.Port);
    }

    public class ReactiveHandler : IReactiveHandler
    {

        public async ValueTask OnConnected(IReactiveConnection connection) => await connection.PingAsync();

        public async ValueTask OnMessage(IReactiveConnection connection, WebsocketFrame message) => await connection.WriteAsync(message.Data);

        public async ValueTask OnContinue(IReactiveConnection connection, WebsocketFrame message) => await connection.WriteAsync(message.Data);

        public async ValueTask OnPing(IReactiveConnection connection, WebsocketFrame message) => await connection.PongAsync(message.Data);

        public async ValueTask OnClose(IReactiveConnection connection, WebsocketFrame message) => await connection.CloseAsync();

        public ValueTask<bool> OnError(IReactiveConnection connection, FrameError error)
        {
            Console.WriteLine($"{error.ErrorType}: {error.Message}");
            return ValueTask.FromResult(false);
        }

    }
    
    public class ReactiveHandlerFragmented : IReactiveHandler
    {

        public ValueTask OnConnected(IReactiveConnection connection) => ValueTask.CompletedTask;

        public async ValueTask OnMessage(IReactiveConnection connection, WebsocketFrame message) => await connection.WriteAsync(message.Data);

        public async ValueTask OnContinue(IReactiveConnection connection, WebsocketFrame message) => await connection.WriteAsync(message.Data);

        public async ValueTask OnPing(IReactiveConnection connection, WebsocketFrame message) => await connection.PongAsync(message.Data);

        public async ValueTask OnClose(IReactiveConnection connection, WebsocketFrame message) => await connection.CloseAsync();

        public ValueTask<bool> OnError(IReactiveConnection connection, FrameError error)
        {
            Console.WriteLine($"{error.ErrorType}: {error.Message}");
            return ValueTask.FromResult(false);
        }

    }

}
