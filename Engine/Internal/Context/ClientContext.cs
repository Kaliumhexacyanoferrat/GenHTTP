using System.IO.Pipelines;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Internal.Protocol;
using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Internal.Context;

internal sealed class ClientContext
{
    private IServer? _server;

    private IEndPoint? _endPoint;

    private NetworkConfiguration? _networkConfiguration;

    private Socket? _connection;

    private X509Certificate? _clientCertificate;

    private Stream? _stream;

    private PipeReader? _reader;
    
    private PipeWriter? _writer;

    private Request _request = new();

    private ResponseHandler _responseHandler;

    private ClientHandler _clientHandler;
    
    internal IServer Server => _server ?? throw new InvalidOperationException("Handler has not been initialized");

    internal IEndPoint EndPoint => _endPoint ?? throw new InvalidOperationException("Handler has not been initialized");

    internal NetworkConfiguration Configuration => _networkConfiguration ?? throw new InvalidOperationException("Handler has not been initialized");

    internal Socket Connection => _connection ?? throw new InvalidOperationException("Handler has not been initialized");

    internal X509Certificate? ClientCertificate => _clientCertificate;

    internal Stream Stream => _stream ?? throw new InvalidOperationException("Handler has not been initialized");

    internal PipeWriter Writer => _writer ?? throw new InvalidOperationException("Handler has not been initialized");
    
    internal PipeReader Reader => _reader ?? throw new InvalidOperationException("Handler has not been initialized");

    internal Request Request => _request;

    internal ResponseHandler ResponseHandler => _responseHandler;
    
    internal ClientHandler ClientHandler => _clientHandler;

    internal ClientContext()
    {
        _clientHandler = new(this);
        _responseHandler = new(this);
    }
    
    internal void Apply(Socket socket, Stream stream, PipeReader reader, X509Certificate? clientCertificate, IServer server, IEndPoint endPoint, NetworkConfiguration config)
    {
        _server = server;
        _endPoint = endPoint;

        _connection = socket;
        _clientCertificate = clientCertificate;

        _networkConfiguration = config;

        _stream = stream;

        _reader = reader;
        _writer = PipeWriter.Create(stream);
    }
    
    public void Reset() => Request.Reset();

}
