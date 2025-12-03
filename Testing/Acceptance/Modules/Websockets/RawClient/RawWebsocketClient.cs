using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.RawClient;

public sealed class RawWebSocketClient : IAsyncDisposable
{
    private readonly Socket _socket;

    public RawWebSocketClient()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task ConnectAsync(string host, int port, CancellationToken token = default)
    {
        await _socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));

        // Build WebSocket handshake
        var keyBytes = new byte[16];
        RandomNumberGenerator.Fill(keyBytes);
        var secWebSocketKey = Convert.ToBase64String(keyBytes);

        var request =
            $"GET / HTTP/1.1\r\n" +
            $"Host: {host}:{port}\r\n" +
            "Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            $"Sec-WebSocket-Key: {secWebSocketKey}\r\n" +
            "Sec-WebSocket-Version: 13\r\n" +
            "\r\n";

        var requestBytes = Encoding.ASCII.GetBytes(request);
        await _socket.SendAsync(requestBytes, SocketFlags.None, token).ConfigureAwait(false);

        // Read handshake response (simple: read until \r\n\r\n)
        var buffer = new byte[4096];
        var received = 0;
        while (true)
        {
            int read = await _socket.ReceiveAsync(buffer.AsMemory(received), SocketFlags.None, token)
                                    .ConfigureAwait(false);

            if (read == 0)
                throw new InvalidOperationException("Server closed connection during handshake.");

            received += read;

            var headerText = Encoding.ASCII.GetString(buffer, 0, received);
            if (headerText.Contains("\r\n\r\n", StringComparison.Ordinal))
                break;
        }

        // Validate "HTTP/1.1 101" and Sec-WebSocket-Accept here?
    }

    /// <summary>
    /// Build a single client->server WebSocket frame (masked as required for clients).
    /// </summary>
    public static byte[] BuildClientFrame(ReadOnlySpan<byte> payload, byte opcode, bool fin)
    {
        const byte MaskBit = 0x80;

        byte b0 = 0;
        if (fin)
            b0 |= 0x80;
        b0 |= (byte)(opcode & 0x0F); // lower 4 bits

        var payloadLen = payload.Length;

        int headerLen;
        if (payloadLen <= 125)
            headerLen = 2 + 4;        // base header + mask
        else if (payloadLen <= ushort.MaxValue)
            headerLen = 2 + 2 + 4;    // base + 16-bit len + mask
        else
            headerLen = 2 + 8 + 4;    // base + 64-bit len + mask

        var frame = new byte[headerLen + payloadLen];
        int offset = 0;

        // First byte
        frame[offset++] = b0;

        // Second byte + extended length
        if (payloadLen <= 125)
        {
            frame[offset++] = (byte)(MaskBit | (byte)payloadLen);
        }
        else if (payloadLen <= ushort.MaxValue)
        {
            frame[offset++] = (byte)(MaskBit | 126);
            BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(offset, 2), (ushort)payloadLen);
            offset += 2;
        }
        else
        {
            frame[offset++] = (byte)(MaskBit | 127);
            BinaryPrimitives.WriteUInt64BigEndian(frame.AsSpan(offset, 8), (ulong)payloadLen);
            offset += 8;
        }

        // Mask key
        var maskKey = new byte[4];
        RandomNumberGenerator.Fill(maskKey);
        maskKey.CopyTo(frame, offset);
        offset += 4;

        // Masked payload
        for (int i = 0; i < payloadLen; i++)
        {
            frame[offset + i] = (byte)(payload[i] ^ maskKey[i & 0x03]);
        }

        return frame;
    }

    /// <summary>
    /// Low-level helper: send raw bytes split into TCP chunks.
    /// This is what gives you "partial frames" on the server side.
    /// </summary>
    public async Task SendRawInChunksAsync(byte[] data, int chunkSize, CancellationToken token = default)
    {
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize));

        var offset = 0;

        while (offset < data.Length)
        {
            var toSend = Math.Min(chunkSize, data.Length - offset);
            var segment = new ReadOnlyMemory<byte>(data, offset, toSend);

            await _socket.SendAsync(segment, SocketFlags.None, token).ConfigureAwait(false);

            offset += toSend;
        }
    }

    /// <summary>
    /// Send a *single* WebSocket text frame,
    /// but split the frame's bytes across multiple Socket.Send calls.
    /// </summary>
    public Task SendTextInTcpChunksAsync(string text, int chunkSize, CancellationToken token = default)
    {
        var payload = Encoding.UTF8.GetBytes(text);
        var frame = BuildClientFrame(payload, opcode: 0x1, fin: true);

        return SendRawInChunksAsync(frame, chunkSize, token);
    }

    /// <summary>
    /// Send multiple complete WebSocket text frames in a *single* Socket.Send call.
    /// </summary>
    public async Task SendMultipleTextFramesSingleWriteAsync(
        CancellationToken token = default,
        params string[] messages)
    {
        if (messages is null || messages.Length == 0)
            return;

        // Build all frames
        var frames = new byte[messages.Length][];
        var totalLen = 0;

        for (int i = 0; i < messages.Length; i++)
        {
            var payload = Encoding.UTF8.GetBytes(messages[i]);
            var frame = BuildClientFrame(payload, opcode: 0x1, fin: true);
            frames[i] = frame;
            totalLen += frame.Length;
        }

        // Concatenate into one buffer
        var buffer = new byte[totalLen];
        var offset = 0;

        foreach (var frame in frames)
        {
            Buffer.BlockCopy(frame, 0, buffer, offset, frame.Length);
            offset += frame.Length;
        }

        // Single write: the server will receive multiple WebSocket frames in one read.
        await _socket.SendAsync(buffer, SocketFlags.None, token).ConfigureAwait(false);
    }

    private async Task ReceiveExactAsync(Memory<byte> buffer, CancellationToken token)
    {
        var totalRead = 0;

        while (totalRead < buffer.Length)
        {
            var read = await _socket.ReceiveAsync(
                    buffer.Slice(totalRead),
                    SocketFlags.None,
                    token)
                .ConfigureAwait(false);

            if (read == 0)
            {
                throw new InvalidOperationException("Socket closed while receiving.");
            }

            totalRead += read;
        }
    }

    /// <summary>
    /// Receive a single WebSocket frame and return its text payload.
    /// Assumes server echoes text frames (opcode=1) and uses FIN=1.
    /// Handles unmasked (normal server) and masked (defensive) payloads.
    /// </summary>
    public async Task<string> ReceiveTextFrameAsync(CancellationToken token = default)
    {
        // Read first 2 bytes (b0 + b1)
        var header = new byte[2];
        await ReceiveExactAsync(header, token).ConfigureAwait(false);

        var b0 = header[0];
        var b1 = header[1];

        var fin = (b0 & 0x80) != 0;
        var opcode = (byte)(b0 & 0x0F);

        if (!fin)
        {
            throw new NotSupportedException("Fragmented server frames not supported in this test helper.");
        }

        if (opcode != 0x1)
        {
            // Text only for this helper; you can expand if needed
            throw new NotSupportedException($"Unexpected opcode {opcode} in ReceiveTextFrameAsync.");
        }

        var masked = (b1 & 0x80) != 0; // servers SHOULD NOT mask, but handle just in case
        var len7 = (byte)(b1 & 0x7F);

        long payloadLen = len7;

        if (len7 == 126)
        {
            var lenBytes = new byte[2];
            await ReceiveExactAsync(lenBytes, token).ConfigureAwait(false);
            payloadLen = BinaryPrimitives.ReadUInt16BigEndian(lenBytes);
        }
        else if (len7 == 127)
        {
            var lenBytes = new byte[8];
            await ReceiveExactAsync(lenBytes, token).ConfigureAwait(false);
            payloadLen = (long)BinaryPrimitives.ReadUInt64BigEndian(lenBytes);
        }

        byte[]? maskKey = null;
        if (masked)
        {
            maskKey = new byte[4];
            await ReceiveExactAsync(maskKey, token).ConfigureAwait(false);
        }

        var payload = new byte[payloadLen];
        if (payloadLen > 0)
        {
            await ReceiveExactAsync(payload.AsMemory(), token).ConfigureAwait(false);
        }

        if (masked && maskKey is not null)
        {
            for (var i = 0; i < payload.Length; i++)
            {
                payload[i] ^= maskKey[i & 0x03];
            }
        }

        return Encoding.UTF8.GetString(payload);
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch
        {
            // ignore
        }

        _socket.Dispose();
        return ValueTask.CompletedTask;
    }
}
