using System.Buffers.Text;
using System.Security.Cryptography;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websockets.Protocol;

public static class Handshake
{
    private static readonly ReadOnlyMemory<byte> MagicString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"u8.ToArray();

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
    public static ByteString CreateAcceptKey(ByteString key)
    {
        var totalLength = key.Bytes.Length + MagicString.Length;

        Span<byte> buffer = stackalloc byte[totalLength];

        key.Bytes.Span.CopyTo(buffer);
        MagicString.Span.CopyTo(buffer[key.Bytes.Length..]);

        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(buffer, hash);

        var result = new byte[28];

        Base64.EncodeToUtf8(hash, result, out _, out var written);

        return new(result.AsMemory(0, written));
    }

}
