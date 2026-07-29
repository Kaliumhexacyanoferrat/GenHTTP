using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Types;

using Microsoft.AspNetCore.Http.Features;

namespace GenHTTP.Adapters.AspNetCore.Context;

/// <summary>
/// Pools a <see cref="Context.Request"/>/<see cref="Context.ResponseWriter"/> pair per request.
/// Public: consumed by <c>GenHTTP.Engine.Kestrel</c> as well as this package's own
/// <c>Mapping.Bridge</c>.
/// </summary>
public sealed class ClientContext : IClientContext
{
    private static readonly StreamPipeWriterOptions WriterOptions = new(MemoryPool<byte>.Shared, leaveOpen: true, minimumBufferSize: BufferSize.Write);

    private IServer? _server;

    private IFeatureCollection? _features;

    private readonly Request _request = new();

    private readonly ResponseWriter _responseWriter;

    // Wraps IHttpResponseBodyFeature.Stream in our own, well-behaved PipeWriter (same as the
    // Internal engine's ClientContext). Kestrel's own IHttpResponseBodyFeature.Writer turns out
    // to hand back a zero-length span from GetSpan() on its very first call when used via this
    // low-level IHttpApplication hosting path - which makes the generic
    // IBufferWriter<byte>.Write(ReadOnlySpan<byte>) extension (used by e.g. StringResource)
    // throw. Routing writes through a plain StreamPipeWriter over the same underlying stream
    // sidesteps that entirely.
    private PipeWriter? _bodyWriter;

    private Stream? _bodyStream;

    public IServer Server => _server ?? throw new InvalidOperationException("Handler has not been initialized");

    internal IFeatureCollection Features => _features ?? throw new InvalidOperationException("Handler has not been initialized");

    public Request Request => _request;

    public ResponseWriter ResponseWriter => _responseWriter;

    // Before an upgrade, the response body belongs to Kestrel's own framing (chunked/
    // content-length/h2 DATA frames); after IHttpUpgradeFeature.UpgradeAsync() (see
    // ResponseWriter/Request.SetUpgraded), it must be the raw connection stream instead -
    // that's the same duplex stream IRequest.Upgrade() hands out as a PipeReader.
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
