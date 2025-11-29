using System.Security.Cryptography;
using System.Text;

namespace GenHTTP.Modules.Straculo.Protocol;

public static class Handshake
{
    /*
      The WebSocket handshake is an HTTP-based negotiation that upgrades a normal HTTP connection into a persistent, 
      full-duplex WebSocket channel. The client begins by sending an HTTP GET request with headers such as Upgrade: websocket, 
      Connection: Upgrade, Sec-WebSocket-Version: 13, and a randomly generated Sec-WebSocket-Key. This key is crucial: the server 
      must take it, append the fixed UUID 258EAFA5-E914-47DA-95CA-C5AB0DC85B11, compute a SHA-1 hash of the result, and return 
      the Base64-encoded value in the Sec-WebSocket-Accept header. If the headers are valid and the accept value matches, the 
      server responds with HTTP/1.1 101 Switching Protocols along with Upgrade: websocket and Connection: Upgrade. After this 101 response, 
      both sides stop speaking HTTP entirely and begin exchanging raw WebSocket frames over the same TCP connection, 
      establishing a stateful, bidirectional, message-oriented stream.
     */
    
    public static string CreateAcceptKey(string key)
    {
        const string magicString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(key + magicString));
        
        return Convert.ToBase64String(hash);
    }
}