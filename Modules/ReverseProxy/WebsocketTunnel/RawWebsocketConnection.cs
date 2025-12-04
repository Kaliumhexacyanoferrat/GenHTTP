using System.Buffers;
using System.IO.Pipelines;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace GenHTTP.Modules.ReverseProxy.WebsocketTunnel;

public class RawWebsocketConnection : IAsyncDisposable
{
    private string _url;
    
    private readonly Socket _socket;
    private readonly bool _secure;
    private readonly string _host;
    private readonly int _port;
    private readonly int _rxMaxBufferSize;
    private readonly int _txMaxBufferSize;

    public Stream? Stream { get; private set; }

    public DuplexPipe? Pipe { get; private set; }

    public RawWebsocketConnection(string url, int rxMaxBufferSize = 4096 * 4, int txMaxBufferSize = 4096)
    {
        _url = url;
        _rxMaxBufferSize = rxMaxBufferSize;
        _txMaxBufferSize = txMaxBufferSize;
        (_host, _port, _secure) = GetHostPortAndSecurity(url);
        
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
                minimumReadSize: Math.Min( _rxMaxBufferSize / 4 , 1024 ))),
            PipeWriter.Create(Stream, 
                new StreamPipeWriterOptions(
                    MemoryPool<byte>.Shared, 
                    leaveOpen: true,
                    minimumBufferSize: _txMaxBufferSize)));
    }

    public async Task<bool> TryUpgrade(
        IReadOnlyDictionary<string, string> clientHeaders, 
        string route = "/", 
        CancellationToken token = default)
    {
        if (Pipe is null)
        {
            throw new InvalidOperationException("Not  initialized.");
        }

        var upgradeRequest = 
            $"GET {route} HTTP/1.1\r\n" + 
            $"Host: {_host}:{_port}\r\n";
            /*"Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            $"Sec-WebSocket-Key: {key}\r\n" +
            "Sec-WebSocket-Version: 13\r\n";*/

        foreach (var header in clientHeaders)
        {
            upgradeRequest += $"{header.Key}: {header.Value}\r\n";
        }

        upgradeRequest += "\r\n";
        
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
                    consumed = sequenceReader.Position;   // consumed up to here
                    examined = consumed;

                    // Validate status line + headers
                    var headersText = Encoding.ASCII.GetString(headersSequence.ToArray());
                    
                    return headersText.StartsWith("HTTP/1.1 101", StringComparison.Ordinal);
                }
                else
                {
                    if (result.IsCompleted)
                    {
                        throw new InvalidOperationException(
                            "Connection closed before full WebSocket handshake was received.");
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
    
    private static (string host, int port, bool secure) GetHostPortAndSecurity(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException($"Invalid URL: {url}", nameof(url));

        var secure = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ||
                     uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase);

        int port;
        if (!uri.IsDefaultPort)
        {
            port = uri.Port;
        }
        else
        {
            port = uri.Scheme.ToLowerInvariant() switch
            {
                "https" => 443,
                "wss"   => 443,
                "http"  => 80,
                "ws"    => 80,
                _       => 0
            };
        }

        return (uri.Host, port, secure);
    }

    public async ValueTask DisposeAsync()
    {
        if(Pipe is not null)
            await Pipe.DisposeAsync();
        
        if(Stream is not null)
            await Stream.DisposeAsync();
        
        _socket?.Dispose();
    }
}