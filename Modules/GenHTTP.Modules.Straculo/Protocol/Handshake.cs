using System.Security.Cryptography;
using System.Text;

namespace GenHTTP.Modules.Straculo.Protocol;

public static class Handshake
{
    public static readonly ReadOnlyMemory<byte> WebsocketHandshakePrefix
        = "HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: "u8.ToArray();

    public static readonly ReadOnlyMemory<byte> WebsocketHandshakeSuffix = "\r\n\r\n"u8.ToArray();
    
    public static string CreateAcceptKey(string request)
    {
        // The WebSocket protocol requires a predefined GUID to be appended to the client's Sec-WebSocket-Key.
        const string magicString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        // Convert the request string into a ReadOnlySpan<char> for efficient searching.
        var requestSpan = request.AsSpan();

        // Locate the `Sec-WebSocket-Key` header in the request using case-insensitive search.
        var keyLineStart = requestSpan.IndexOf("Sec-WebSocket-Key:".AsSpan(), StringComparison.OrdinalIgnoreCase);

        // If the key is not found, the request is invalid, and an exception is thrown.
        if (keyLineStart == -1)
        {
            throw new InvalidOperationException("Sec-WebSocket-Key not found in the request.");
        }

        // Extract the portion of the request after `Sec-WebSocket-Key:` (the actual key value follows).
        var keyLine = requestSpan[(keyLineStart + "Sec-WebSocket-Key:".Length)..];

        // Find the end of the key value, which is marked by a newline (`\r\n`).
        var keyEnd = keyLine.IndexOf("\r\n".AsSpan());
        if (keyEnd != -1)
        {
            keyLine = keyLine[..keyEnd]; // Trim the key value to exclude the newline.
        }

        // Trim any surrounding whitespace from the key.
        var key = keyLine.Trim();

        // Concatenate the extracted WebSocket key with the magic string and compute its SHA-1 hash.
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(key.ToString() + magicString));

        // Convert the hash to a Base64 string, as required by the WebSocket protocol.
        return Convert.ToBase64String(hash);
    }
}