using System.Buffers;
using System.IO.Pipelines;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace GenHTTP.Modules.ReverseProxy.Websocket;

public sealed class RawWebsocketConnection : IAsyncDisposable
{
    private readonly Socket _socket;

    private readonly bool _secure; // wss - ws
    private readonly string _host;
    private readonly int _port;
    private readonly string _route; // path, no query params

    private readonly int _rxMaxBufferSize;
    private readonly int _txMaxBufferSize;

    public Stream? Stream { get; private set; }

    public DuplexPipe? Pipe { get; private set; }

    public RawWebsocketConnection(string url, int rxMaxBufferSize = 4096 * 4, int txMaxBufferSize = 4096)
    {
        _rxMaxBufferSize = rxMaxBufferSize;
        _txMaxBufferSize = txMaxBufferSize;
        (_host, _port, _secure, _route) = GetHostPortAndSecurity(url);

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.NoDelay = true;
    }

    public async Task InitializeStream()
    {
        await _socket.ConnectAsync(_host, _port);

        Stream = new NetworkStream(_socket, ownsSocket: true);

        if (_secure)
        {
            var sslStream = new SslStream(Stream, false);
            await sslStream.AuthenticateAsClientAsync(_host);
            Stream = sslStream;
        }

        Pipe = new DuplexPipe(PipeReader.Create(Stream,
                                                new StreamPipeReaderOptions(
                                                    MemoryPool<byte>.Shared,
                                                    leaveOpen: true,
                                                    bufferSize: _rxMaxBufferSize,
                                                    minimumReadSize: Math.Min(_rxMaxBufferSize / 4, 1024))),
                              PipeWriter.Create(Stream,
                                                new StreamPipeWriterOptions(
                                                    MemoryPool<byte>.Shared,
                                                    leaveOpen: true,
                                                    minimumBufferSize: _txMaxBufferSize)));
    }

    public async Task<bool> TryUpgrade(
        IReadOnlyDictionary<string, string> clientHeaders,
        CancellationToken token = default)
    {
        if (Pipe is null)
        {
            throw new InvalidOperationException("Not initialized.");
        }

        var upgradeRequest =
            $"GET {_route} HTTP/1.1\r\n" +
            $"Host: {_host}:{_port}\r\n";

        foreach (var header in clientHeaders)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                continue;

            upgradeRequest += $"{header.Key}: {header.Value}\r\n";
        }

        upgradeRequest += "\r\n";

        Console.WriteLine($"Upgrade Request (Server -> Upstream): {upgradeRequest}");

        // Writes and flushes
        await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes(upgradeRequest), token);

        // Read the handshake response until \r\n\r\n
        while (true)
        {
            var result = await Pipe.Input.ReadAsync(token);
            var buffer = result.Buffer;

            if (result.IsCompleted && buffer.Length == 0)
            {
                throw new InvalidOperationException(
                    "Connection closed before full WebSocket handshake was received.");
            }

            var sequenceReader = new SequenceReader<byte>(buffer);

            SequencePosition consumed = buffer.Start;
            SequencePosition examined = buffer.End;

            try
            {
                if (sequenceReader.TryReadTo(
                        out ReadOnlySequence<byte> headersSequence,
                        "\r\n\r\n"u8,
                        advancePastDelimiter: true))
                {
                    consumed = sequenceReader.Position; // consumed up to here
                    examined = consumed;

                    // Validate status line + headers
                    var headersText = Encoding.ASCII.GetString(headersSequence.ToArray());

                    Console.WriteLine($"Received Handshake (Upstream -> Server): {headersText}");

                    return headersText.StartsWith("HTTP/1.1 101", StringComparison.Ordinal);
                }
                else
                {
                    if (result.IsCompleted)
                    {
                        throw new InvalidOperationException("Connection closed before full WebSocket handshake was received.");
                    }
                }
            }
            finally
            {
                // TODO: Does it make sense to advance in case of exception?
                Pipe.Input.AdvanceTo(consumed, examined);
            }
        }
    }

    private static (string host, int port, bool secure, string route) GetHostPortAndSecurity(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException($"Invalid URL: {url}", nameof(url));

        bool secure = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ||
            uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase);

        int port = !uri.IsDefaultPort
            ? uri.Port
            : uri.Scheme.ToLowerInvariant() switch
            {
                "https" => 443,
                "wss" => 443,
                "http" => 80,
                "ws" => 80,
                _ => 0
            };

        // Extract only the path, ensure at least "/"
        string route = string.IsNullOrEmpty(uri.AbsolutePath) ? "/" : uri.AbsolutePath;

        return (uri.Host, port, secure, route);
    }

    public async ValueTask DisposeAsync()
    {
        if (Pipe is not null)
            await Pipe.DisposeAsync();

        if (Stream is not null)
            await Stream.DisposeAsync();

        _socket.Dispose();
    }
    
}
