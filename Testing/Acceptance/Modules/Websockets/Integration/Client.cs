using GenHTTP.Testing.Acceptance.Modules.Websockets.RawClient;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration;

public static class Client
{
    public static async ValueTask Execute(int port)
    {
        using var cts = new CancellationTokenSource(4000);
        var token = cts.Token;

        var client = new ClientWebSocket();

        await client.ConnectAsync(new Uri($"ws://localhost:{port}"), token);

        // Sending a Text message
        var bytes = "Hello, World!"u8.ToArray();
        await client.SendAsync(bytes, WebSocketMessageType.Text, true, token);

        var responseBuffer = new byte[bytes.Length];
        var response = await client.ReceiveAsync(new ArraySegment<byte>(responseBuffer), token);

        Assert.AreEqual(WebSocketMessageType.Text, response.MessageType);
        Assert.AreEqual("Hello, World!", Encoding.UTF8.GetString(responseBuffer));

        // Sending a fragmented Text message
        //
        // First frame
        //
        var firstFrame = "This is the first segment"u8.ToArray();
        var firstResponseBuffer = new byte[firstFrame.Length];

        await client.SendAsync(firstFrame, WebSocketMessageType.Text, false, token);
        response = await client.ReceiveAsync(new ArraySegment<byte>(firstResponseBuffer), token);

        Assert.AreEqual(WebSocketMessageType.Text, response.MessageType);
        Assert.AreEqual("This is the first segment", Encoding.UTF8.GetString(firstResponseBuffer));
        //
        // Second frame
        //
        var secondFrame = "This is the second segment"u8.ToArray();
        var secondResponseBuffer = new byte[secondFrame.Length];

        await client.SendAsync(secondFrame, WebSocketMessageType.Text, true, token);
        response = await client.ReceiveAsync(new ArraySegment<byte>(secondResponseBuffer), token);

        Assert.AreEqual(WebSocketMessageType.Text, response.MessageType);
        Assert.AreEqual("This is the second segment", Encoding.UTF8.GetString(secondResponseBuffer));

        // Close connection
        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", token);

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
    
    public static async ValueTask ExecuteSegmented(int port)
    {
        using var cts = new CancellationTokenSource(4000);
        var token = cts.Token;

        var client = new ClientWebSocket();

        await client.ConnectAsync(new Uri($"ws://localhost:{port}"), token);

        // Sending a Text message
        var bytes = "Hello, World!"u8.ToArray();
        await client.SendAsync(bytes, WebSocketMessageType.Text, true, token);

        var responseBuffer = new byte[bytes.Length];
        var response = await client.ReceiveAsync(new ArraySegment<byte>(responseBuffer), token);

        Assert.AreEqual(WebSocketMessageType.Text, response.MessageType);
        Assert.AreEqual("Hello, World!", Encoding.UTF8.GetString(responseBuffer));

        // Sending a fragmented Text message
        //
        // First frame
        //
        var firstFrame = "First segment - "u8.ToArray();
        await client.SendAsync(firstFrame, WebSocketMessageType.Text, false, token);
        //
        // Second frame
        //
        var secondFrame = "Second segment"u8.ToArray();
        responseBuffer = new byte[firstFrame.Length + secondFrame.Length];

        await client.SendAsync(secondFrame, WebSocketMessageType.Text, true, token);
        response = await client.ReceiveAsync(new ArraySegment<byte>(responseBuffer), token);

        Assert.AreEqual(WebSocketMessageType.Text, response.MessageType);
        Assert.AreEqual("First segment - Second segment", Encoding.UTF8.GetString(responseBuffer));

        // Close connection
        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", token);

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

    public static async ValueTask ExecuteFragmented(string host, int port)
    {
        using var cts = new CancellationTokenSource(5000);
        var token = cts.Token;

        await using var client = new RawWebSocketClient();
        await client.ConnectAsync(host, port, token);

        // Single WebSocket frame fragmented over TCP writes
        await client.SendTextInTcpChunksAsync("Hello over multiple sends!", chunkSize: 3, token);

        // Multiple frames in a single TCP write
        await client.SendMultipleTextFramesSingleWriteAsync(
            token,
            "First frame",
            "Second frame",
            "Third frame");

        // Read echoes: we expect 4 frames back
        var echo1 = await client.ReceiveTextFrameAsync(token);
        var echo2 = await client.ReceiveTextFrameAsync(token);
        var echo3 = await client.ReceiveTextFrameAsync(token);
        var echo4 = await client.ReceiveTextFrameAsync(token);

        Assert.AreEqual("Hello over multiple sends!", echo1);
        Assert.AreEqual("First frame", echo2);
        Assert.AreEqual("Second frame", echo3);
        Assert.AreEqual("Third frame", echo4);
    }
}
