using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Types;

using Microsoft.AspNetCore.Http.Features;

namespace GenHTTP.Adapters.AspNetCore.Context;

/// <summary>
/// Pools a <see cref="Context.Request"/>/<see cref="Context.ResponseWriter"/> pair per request.
/// </summary>
public sealed class ClientContext : IClientContext
{
    private static readonly StreamPipeWriterOptions WriterOptions = new(MemoryPool<byte>.Shared, leaveOpen: true, minimumBufferSize: BufferSize.Write);

    private IServer? _server;

    private IFeatureCollection? _features;

    private readonly Request _request = new();

    private readonly ResponseWriter _responseWriter;

    // Wraps IHttpResponseBodyFeature.Stream in our own PipeWriter: Kestrel's own
    // IHttpResponseBodyFeature.Writer hands back a zero-length span from GetSpan() on its
    // first call in this hosting path, which breaks IBufferWriter<byte>.Write(...).
    private PipeWriter? _bodyWriter;

    private Stream? _bodyStream;

    public IServer Server => _server ?? throw new InvalidOperationException("Handler has not been initialized");

    internal IFeatureCollection Features => _features ?? throw new InvalidOperationException("Handler has not been initialized");

    public Request Request => _request;

    public ResponseWriter ResponseWriter => _responseWriter;

    // Before an upgrade, the body belongs to Kestrel's own framing; after Request.SetUpgraded()
    // it must be the same raw connection stream IRequest.Upgrade() hands out as a PipeReader.
    public Stream Stream => _request.UpgradedStream ?? (_bodyStream ??= Features.GetRequiredFeature<IHttpResponseBodyFeature>().Stream);

    public PipeWriter Writer => _request.UpgradedWriter ?? (_bodyWriter ??= PipeWriter.Create(Stream, WriterOptions));

    public ClientContext()
    {
        _responseWriter = new(this);
    }

    public void Apply(IServer server, IFeatureCollection features)
    {
        _server = server;
        _features = features;
    }

    public void Reset()
    {
        _server = null;

        _features = null;

        _bodyWriter = null;
        _bodyStream = null;

        Request.Reset();
    }

}
