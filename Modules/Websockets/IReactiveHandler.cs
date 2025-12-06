using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets;

public interface IReactiveHandler
{

    ValueTask OnConnected(IReactiveConnection connection) => ValueTask.CompletedTask;
    
    ValueTask OnMessage(IReactiveConnection connection, WebsocketFrame message) =>  ValueTask.CompletedTask;
    
    ValueTask OnBinary(IReactiveConnection connection, WebsocketFrame message) =>  ValueTask.CompletedTask;
    
    ValueTask OnContinue(IReactiveConnection connection, WebsocketFrame message) =>  ValueTask.CompletedTask;

    ValueTask OnPing(IReactiveConnection connection, WebsocketFrame message) =>  ValueTask.CompletedTask;
    
    ValueTask OnPong(IReactiveConnection connection, WebsocketFrame message) =>  ValueTask.CompletedTask;
    
    ValueTask OnClose(IReactiveConnection connection, WebsocketFrame message) =>  ValueTask.CompletedTask;

    ValueTask<bool> OnError(IReactiveConnection connection, FrameError error) => ValueTask.FromResult(true);

}
