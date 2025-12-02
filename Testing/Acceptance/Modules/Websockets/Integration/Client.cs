using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Websocket.Client;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Integration;

public static class Client
{
    private static async Task SendWebSocketFrame(Socket socket, string message, bool isFinal, CancellationToken token)
    {
        var payload = Encoding.UTF8.GetBytes(message);
        var frame = new List<byte>();

        // FIN bit and opcode (0x1 for text frame, 0x0 for continuation)
        byte firstByte = (byte)((isFinal ? 0x80 : 0x00) | 0x01);
        frame.Add(firstByte);

        // Mask bit (client must mask) and payload length
        byte secondByte;
        if (payload.Length < 126)
        {
            secondByte = (byte)(0x80 | payload.Length);
            frame.Add(secondByte);
        }
        else if (payload.Length <= ushort.MaxValue)
        {
            secondByte = 0x80 | 126;
            frame.Add(secondByte);
            frame.Add((byte)(payload.Length >> 8));
            frame.Add((byte)(payload.Length & 0xFF));
        }
        else
        {
            secondByte = 0x80 | 127;
            frame.Add(secondByte);
            for (int i = 7; i >= 0; i--)
            {
                frame.Add((byte)((payload.Length >> (i * 8)) & 0xFF));
            }
        }

        // Masking key (4 random bytes)
        var maskingKey = new byte[4];
        Random.Shared.NextBytes(maskingKey);
        frame.AddRange(maskingKey);

        // Masked payload
        for (int i = 0; i < payload.Length; i++)
        {
            frame.Add((byte)(payload[i] ^ maskingKey[i % 4]));
        }

        await socket.SendAsync(frame.ToArray(), SocketFlags.None, token);
    }

    private static async Task<string> ReceiveWebSocketFrame(Socket socket, CancellationToken token)
    {
        var header = new byte[2];
        await socket.ReceiveAsync(header, SocketFlags.None, token);

        bool isFinal = (header[0] & 0x80) != 0;
        bool isMasked = (header[1] & 0x80) != 0;
        int payloadLength = header[1] & 0x7F;

        // Read extended payload length if needed
        if (payloadLength == 126)
        {
            var extendedLength = new byte[2];
            await socket.ReceiveAsync(extendedLength, SocketFlags.None, token);
            payloadLength = (extendedLength[0] << 8) | extendedLength[1];
        }
        else if (payloadLength == 127)
        {
            var extendedLength = new byte[8];
            await socket.ReceiveAsync(extendedLength, SocketFlags.None, token);
            payloadLength = 0;
            for (int i = 0; i < 8; i++)
            {
                payloadLength = (payloadLength << 8) | extendedLength[i];
            }
        }

        // Read masking key if present
        byte[] maskingKey = null!;
        if (isMasked)
        {
            maskingKey = new byte[4];
            await socket.ReceiveAsync(maskingKey, SocketFlags.None, token);
        }

        // Read payload
        var payload = new byte[payloadLength];
        int totalRead = 0;
        while (totalRead < payloadLength)
        {
            var read = await socket.ReceiveAsync(payload.AsMemory(totalRead), SocketFlags.None, token);
            totalRead += read;
        }

        // Unmask if needed
        if (isMasked)
        {
            for (int i = 0; i < payload.Length; i++)
            {
                payload[i] ^= maskingKey[i % 4];
            }
        }

        return Encoding.UTF8.GetString(payload);
    }

    private static async Task SendCloseFrame(Socket socket, CancellationToken token)
    {
        var frame = new List<byte>
        {
            0x88, // FIN + Close opcode
            0x82  // Mask + payload length 2
        };

        // Masking key
        var maskingKey = new byte[4];
        Random.Shared.NextBytes(maskingKey);
        frame.AddRange(maskingKey);

        // Status code 1000 (normal closure), masked
        frame.Add((byte)(0x03 ^ maskingKey[0])); // High byte of 1000
        frame.Add((byte)(0xE8 ^ maskingKey[1])); // Low byte of 1000

        await socket.SendAsync(frame.ToArray(), SocketFlags.None, token);
    }
    
    public static async ValueTask Execute(int port)
    {
        var cts = new CancellationTokenSource(2000);
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

    //private static CancellationToken TimeoutToken(int ms = 2000) => new CancellationTokenSource(ms).Token;

}
