using Microsoft.VisualStudio.TestTools.UnitTesting;
using WS = GenHTTP.Modules.Websockets;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets;

[TestClass]
public class InitializerTest
{

    [TestMethod]
    public void InitializeAll()
    {
        WS.Websocket.Create()
          .OnOpen(s => { })
          .OnClose(s => { })
          .OnPing((s, b) => { s.SendPong(b); })
          .OnPong((s, b) => { })
          .OnMessage((s, x) => { })
          .OnBinary((s, x) => { })
          .OnError((s, x) => { })
          .Protocol("chat");
    }
    
}
