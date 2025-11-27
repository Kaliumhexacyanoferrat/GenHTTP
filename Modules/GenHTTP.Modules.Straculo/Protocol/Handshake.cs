using System.Security.Cryptography;
using System.Text;

namespace GenHTTP.Modules.Straculo.Protocol;

public static class Handshake
{
    public static string CreateAcceptKey(string key)
    {
        // The WebSocket protocol requires a predefined GUID to be appended to the client's Sec-WebSocket-Key.
        const string magicString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        
        // Concatenate the extracted WebSocket key with the magic string and compute its SHA-1 hash.
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(key + magicString));

        // Convert the hash to a Base64 string, as required by the WebSocket protocol.
        return Convert.ToBase64String(hash);
    }
}