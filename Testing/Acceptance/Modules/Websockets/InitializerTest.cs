using WS = GenHTTP.Modules.Websockets;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets;

[TestClass]
public class InitializerTest
{

    [TestMethod]
    public void InitializeAll()
    {
        WS.Websocket.Create()
          .OnOpen(s => Task.CompletedTask)
          .OnClose(s => Task.CompletedTask)
          .OnPing(async (s, b) => { await s.SendPongAsync(b); })
          .OnPong((s, b) => Task.CompletedTask)
          .OnMessage((s, x) => Task.CompletedTask)
          .OnBinary((s, x) => Task.CompletedTask)
          .OnError((s, x) => Task.CompletedTask)
          .Protocol("chat");
    }
    
}
