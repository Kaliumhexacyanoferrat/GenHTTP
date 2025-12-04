using System.Buffers;
using System.IO.Pipelines;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Handler;

namespace GenHTTP.Modules.ReverseProxy;

public class RawWebsocketConnection : IDisposable
{
    private string _url;
    
    private readonly Socket _raw;
    private readonly bool _secure;
    private readonly string _host;
    private readonly int _port;
    private readonly int _rxMaxBufferSize;
    private readonly int _txMaxBufferSize;

    public Stream Stream { get; private set; } = null!;
    public PipeReader? Reader { get; private set; }
    public PipeWriter? Writer { get; private set; }

    public RawWebsocketConnection(string url, int rxMaxBufferSize = 4096 * 4, int txMaxBufferSize = 4096)
    {
        _url = url;
        _rxMaxBufferSize = rxMaxBufferSize;
        _txMaxBufferSize = txMaxBufferSize;
        (_host, _port, _secure) = GetHostPortAndSecurity(url);
        
        _raw = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _raw.NoDelay = true;
    }

    public async Task InitializeStream()
    {
        await _raw.ConnectAsync(_host, _port);
        
        Stream = new NetworkStream(_raw, ownsSocket: true);
        
        if (_secure)
        {
            var sslStream = new SslStream(Stream, false);
            await sslStream.AuthenticateAsClientAsync(_host);
            Stream = sslStream;
        }
        
        Reader = PipeReader.Create(Stream, 
            new StreamPipeReaderOptions(
            MemoryPool<byte>.Shared, 
            leaveOpen: true,
            bufferSize: _rxMaxBufferSize, 
            minimumReadSize: Math.Min( _rxMaxBufferSize / 4 , 1024 )));

        Writer = PipeWriter.Create(Stream, 
            new StreamPipeWriterOptions(
                MemoryPool<byte>.Shared, 
                leaveOpen: true,
                minimumBufferSize: _txMaxBufferSize));
    }

    public async Task<bool> TryUpgrade(string key, string? extensions = null, CancellationToken token = default)
    {
        if (Writer is null || Reader is null)
        {
            throw new InvalidOperationException("Not  initialized.");
        }
        
        var upgradeRequest =
            $"GET / HTTP/1.1\r\n" +
            $"Host: {_host}:{_port}\r\n" +
            "Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            $"Sec-WebSocket-Key: {key}\r\n" +
            "Sec-WebSocket-Version: 13\r\n";
        if (extensions != null)
        {
            upgradeRequest += $"Sec-WebSocket-Extensions: {extensions}\r\n\r\n";
        }
        else
        {
            upgradeRequest += "\r\n";
        }
        
        // Writes and flushes
        await Writer.WriteAsync(Encoding.UTF8.GetBytes(upgradeRequest), token);
        
        // Read the handshake response until \r\n\r\n
        while (true)
        {
            var result = await Reader.ReadAsync(token);
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
                Reader.AdvanceTo(consumed, examined);
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

    public void Dispose()
    {
        Reader?.Complete();
        Writer?.Complete();
        
        Stream?.Dispose();
        
        _raw?.Dispose();
    }
}

public class WebsocketProxy : IHandler
{
    private readonly string _upstreamUrl;
    
    public WebsocketProxy(string upstreamUrl)
    {
        _upstreamUrl = upstreamUrl;
    }
    
    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        // This logic is redundant here, should be moved to this method's caller
        if (!request.Headers.ContainsKey("Upgrade") || request.Headers["Upgrade"] != "websocket")
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Websocket upgrade request expected");
        }

        var upgradeInfo = request.Upgrade();
        upgradeInfo.Socket.NoDelay = true;
        
        // Frozen request
        var clone = ClonedRequest.From(request);
        
        var upstreamConnection = new RawWebsocketConnection(_upstreamUrl);
        await upstreamConnection.InitializeStream();
        
        // Establish a websocket with upstream - use same key
        var upgradeCts = new CancellationTokenSource(5000);
        var upstreamUpgradedSuccessfully = false;
        var key = clone.Headers["Sec-WebSocket-Key"];
        if (clone.Headers.TryGetValue("Sec-WebSocket-Extensions", out var extensions))
        {
            upstreamUpgradedSuccessfully = await upstreamConnection.TryUpgrade(key, extensions, upgradeCts.Token);
        }
        else
        {
            upstreamUpgradedSuccessfully = await upstreamConnection.TryUpgrade(key, token: upgradeCts.Token);
        }

        if (!upstreamUpgradedSuccessfully)
        {
            throw new InvalidOperationException("Failed to upgrade upstream.");
        }
        
        // Websocket connection with upstream is Ok, return this to establish websocket connection with downstream too
        
        return upgradeInfo.Response;
    }

    // Pure delegates
    public static Func<Stream, Task> UpstreamHandler = async upstream =>
    {
        await Task.Delay(1);
    };

    public static Func<Stream, Task> DownstreamHandler = async stream =>
    {
        await Task.Delay(1);
    };
}