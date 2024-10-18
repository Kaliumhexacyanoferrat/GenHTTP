using Fleck;
using GenHTTP.Engine;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Websockets;

var allSockets = new List<IWebSocketConnection>();

var websocket = Websocket.Create()
                         .Handler((socket) =>
                         {
                             socket.OnOpen = () =>
                             {
                                 Console.WriteLine("Open!");
                                 allSockets.Add(socket);
                             };
                             socket.OnClose = () =>
                             {
                                 Console.WriteLine("Close!");
                                 allSockets.Remove(socket);
                             };
                             socket.OnMessage = message =>
                             {
                                 Console.WriteLine(message);
                                 allSockets.ToList().ForEach(s => s.Send("Echo: " + message));
                             };
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
    foreach (var socket in allSockets.ToList())
    {
        await socket.Send(input);
    }

    input = Console.ReadLine();
}

host.Stop();
