using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration;

public static class Client
{

    public static async ValueTask Execute(int port)
    {
        var client = new ClientWebSocket();

        await client.ConnectAsync(new Uri($"ws://localhost:{port}"), TimeoutToken());

        // Sending a Text message
        var bytes = "Hello, World!"u8.ToArray();
        await client.SendAsync(bytes, WebSocketMessageType.Text, true, TimeoutToken());

        var responseBuffer = new byte[bytes.Length];
        var response = await client.ReceiveAsync(new ArraySegment<byte>(responseBuffer), TimeoutToken());

        Assert.AreEqual(WebSocketMessageType.Text, response.MessageType);
        Assert.AreEqual("Hello, World!", Encoding.UTF8.GetString(responseBuffer));

        // Sending a fragmented Text message
        //
        // First frame
        //
        var firstFrame = "This is the first segment"u8.ToArray();
        var firstResponseBuffer = new byte[firstFrame.Length];

        await client.SendAsync(firstFrame, WebSocketMessageType.Text, false, TimeoutToken());
        response = await client.ReceiveAsync(new ArraySegment<byte>(firstResponseBuffer), TimeoutToken());

        Assert.AreEqual(WebSocketMessageType.Text, response.MessageType);
        Assert.AreEqual("This is the first segment", Encoding.UTF8.GetString(firstResponseBuffer));
        //
        // Second frame
        //
        var secondFrame = "This is the second segment"u8.ToArray();
        var secondResponseBuffer = new byte[secondFrame.Length];

        await client.SendAsync(secondFrame, WebSocketMessageType.Text, true, TimeoutToken());
        response = await client.ReceiveAsync(new ArraySegment<byte>(secondResponseBuffer), TimeoutToken());

        Assert.AreEqual(WebSocketMessageType.Text, response.MessageType);
        Assert.AreEqual("This is the second segment", Encoding.UTF8.GetString(secondResponseBuffer));

        // Close connection
        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", TimeoutToken());

        try
        {
            client.Dispose();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            Console.WriteLine(e);
        }
    }

    private static CancellationToken TimeoutToken(int ms = 2000) => new CancellationTokenSource(ms).Token;

}
