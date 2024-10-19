using GenHTTP.Engine;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Websockets;

var allSockets = new List<IWebsocketConnection>();

var websocket = Websocket.Create()
                         .OnOpen((socket) =>
                         {
                             Console.WriteLine("Open!");
                             allSockets.Add(socket);
                         })
                         .OnClose((socket) =>
                         {
                             Console.WriteLine("Close!");
                             allSockets.Remove(socket);
                         })
                         .OnMessage((socket, message) =>
                         {
                             Console.WriteLine(message);
                             allSockets.ToList().ForEach(s => s.Send("Echo: " + message));
                         });

var host = Host.Create()
    .Handler(websocket)
    .Defaults()
    .Development()
    .Console();

host.Start();

var input = Console.ReadLine();

while (input != "exit")
{
    if (input != null)
    {
        foreach (var socket in allSockets.ToList())
        {
            await socket.Send(input);
        }
    }

    input = Console.ReadLine();
}

host.Stop();
