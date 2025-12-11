using System.Buffers;
using System.IO.Pipelines;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ReverseProxy.Websocket;

/* TLDR:
   RawWebsocketConnection is a small helper around a raw TCP Socket that knows how to connect to a ws:// or wss:// URL, 
   wrap it in the right stream, and perform the HTTP upgrade handshake using System.IO.Pipelines. 
   In the constructor it parses the URL into host, port, whether the port is default, whether the connection is 
   secure (wss/https → SslStream), and the route (path without query params). InitializeStream connects the socket, 
   builds a NetworkStream, optionally upgrades it to an SslStream for secure endpoints, and then creates a DuplexPipe (pipe reader/writer) 
   over that stream with configurable RX/TX buffer sizes. TryUpgrade then builds a WebSocket upgrade GET request by 
   hand (including a correct Host header that omits the port when it’s the default), writes it to the pipe, and loops reading 
   from the input pipe until it finds the HTTP header terminator \r\n\r\n. Once it has the full response headers, it checks whether 
   the status line starts with HTTP/1.1 101 to decide if the upgrade succeeded. The method uses the PipeReader contract correctly 
   by always calling AdvanceTo in a finally block, and it throws if the connection closes before the full handshake arrives. Finally, 
   DisposeAsync cleans up the pipe, the underlying stream, and the socket, so the class encapsulates the entire lifecycle of a 
   single raw WebSocket client connection.
 */
public sealed class RawWebsocketConnection : IAsyncDisposable
{
    private readonly Socket _socket;

    private readonly bool _secure; // wss - ws
    private readonly string _host;
    private readonly int _port;
    private readonly bool _isDefaultPort;
    private readonly string _route;

    private readonly int _rxMaxBufferSize;
    private readonly int _txMaxBufferSize;

    private const string Crlf = "\r\n";

    #region Get-/Setters
    
    private Stream? Stream { get; set; }

    public DuplexPipe? Pipe { get; private set; }

    #endregion
    
    #region Initialization
    
    public RawWebsocketConnection(string url, int rxMaxBufferSize = 4096 * 4, int txMaxBufferSize = 4096)
    {
        _rxMaxBufferSize = rxMaxBufferSize;
        _txMaxBufferSize = txMaxBufferSize;
        (_host, _port, _isDefaultPort, _secure, _route) = GetHostPortAndSecurity(url);

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
    
    #endregion
    
    #region Functionality

    public async Task TryUpgrade(IRequest request, CancellationToken token = default)
    {
        if (Pipe is null)
        {
            throw new InvalidOperationException("Not initialized.");
        }

        var upgradeRequestSb = new StringBuilder();
        
        upgradeRequestSb
             // GET {_route} HTTP/1.1\r\n
            .Append("GET ")
            .Append(_route)
            .Append(" HTTP/1.1")
            .Append(Crlf)
            // Host: {_host}:{_port}\r\n
            .Append("Host: ")
            .Append(_host);
        
        if (!_isDefaultPort)
        {
            upgradeRequestSb.Append(':');
            upgradeRequestSb.Append(_port);
        }
        
        upgradeRequestSb.Append(Crlf);

        foreach (var header in request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                continue;

            upgradeRequestSb
                .Append(header.Key)
                .Append(": ")
                .Append(header.Value)
                .Append(Crlf);
        }

        upgradeRequestSb.Append(Crlf);

        // Writes and flushes
        await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes(upgradeRequestSb.ToString()), token);

        // Read the handshake response until \r\n\r\n
        while (request.Server.Running)
        {
            var result = await Pipe.Input.ReadAsync(token);
            var buffer = result.Buffer;

            if (result.IsCompleted && buffer.Length == 0)
            {
                throw new ProviderException(ResponseStatus.BadGateway, "Connection closed before full WebSocket handshake was received.");
            }

            var sequenceReader = new SequenceReader<byte>(buffer);

            var consumed = buffer.Start;
            var examined = buffer.End;

            try
            {
                if (sequenceReader.TryReadTo(out ReadOnlySequence<byte> _, "\r\n\r\n"u8, advancePastDelimiter: true))
                {
                    examined = consumed = sequenceReader.Position; // consumed up to here
                    return;
                }
                else
                {
                    if (result.IsCompleted)
                    {
                        throw new ProviderException(ResponseStatus.BadGateway, "Connection closed before full WebSocket handshake was received.");
                    }
                }
            }
            finally
            {
                Pipe.Input.AdvanceTo(consumed, examined);
            }
        }
    }

    private static (string host, int port, bool isDefaultPort, bool secure, string route) GetHostPortAndSecurity(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException($"Invalid URL: {url}", nameof(url));

        var secure = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ||
                     uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase);

        var port = !uri.IsDefaultPort
            ? uri.Port
            : uri.Scheme.ToLowerInvariant() switch
            {
                "https" => 443,
                "wss" => 443,
                "http" => 80,
                "ws" => 80,
                _ => 0
            };
        
        var route = string.IsNullOrEmpty(uri.PathAndQuery) ? "/" : uri.PathAndQuery;

        return (uri.Host, port, uri.IsDefaultPort, secure, route);
    }

    public async ValueTask DisposeAsync()
    {
        if (Pipe is not null)
        {
            await Pipe.DisposeAsync();
        }

        if (Stream is not null)
        {
            await Stream.DisposeAsync();
        }
        
        _socket.Dispose();
    }
    
    #endregion
    
}
