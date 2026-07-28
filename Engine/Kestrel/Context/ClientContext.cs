using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Kestrel.Integration;
using GenHTTP.Engine.Shared.Types;

using Microsoft.AspNetCore.Http.Features;

namespace GenHTTP.Engine.Kestrel.Context;

internal sealed class ClientContext : IClientContext
{
    private IServer? _server;

    private IFeatureCollection? _features;

    private readonly Request _request = new();

    private readonly RequestHandler _requestHandler;

    private readonly ResponseHandler _responseHandler;

    public IServer Server => _server ?? throw new InvalidOperationException("Handler has not been initialized");

    internal IFeatureCollection Features => _features ?? throw new InvalidOperationException("Handler has not been initialized");

    internal Request Request => _request;

    internal ResponseHandler ResponseHandler => _responseHandler;

    internal RequestHandler RequestHandler => _requestHandler;

    internal ClientContext()
    {
        _requestHandler = new(this);
        _responseHandler = new(this);
    }

    internal void Apply(IServer server, IFeatureCollection features)
    {
        _server = server;
        _features = features;
    }

    public void Reset()
    {
        _server = null;

        _features = null;

        Request.Reset();
    }

}
