using System.IO.Pipelines;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types.Body;

namespace GenHTTP.Engine.Shared.Types;

internal enum ConsumptionStrategy
{
    Stream,
    Memory
}

public class RequestBody(IRequest request) : IRequestBody
{
    private static readonly ReadOnlyMemory<byte> ChunkedValue = "chunked"u8.ToArray();

    private ConsumptionStrategy? _strategy;

    private readonly StreamConsumptionStrategy _streamStrategy = new();

    private readonly MemoryConsumptionStrategy _memoryStrategy = new();
    
    private PipeReader? _reader;

    #region Get-/Setters

    public ContentType? Type { get; private set; }

    public ReadOnlyMemory<byte>? Encoding { get; private set; }

    public ulong? Length { get; private set; }

    internal PipeReader Reader => _reader ?? throw new InvalidOperationException("Reader has not been initialized");
    
    #endregion

    #region Initialization

    public void Apply(PipeReader reader)
    {
        _reader = reader;
        
        // request headers might be released so we need to persist the values here
        var headers = request.Header.Headers;

        var contentType = headers.GetEntry(KnownHeaders.ContentType);

        Type = (contentType != null) ? new(contentType.Value.ToArray()) : null;

        Encoding = headers.GetEntry(KnownHeaders.ContentEncoding)?.ToArray();

        var contentLength = headers.GetEntry(KnownHeaders.ContentLength);

        if (contentLength != null)
        {
            if (ulong.TryParse(contentLength.Value.Span, out var longValue))
            {
                Length = longValue;
            }
            else
            {
                throw new ProviderException(ResponseStatus.BadRequest, "Content-Length header has an invalid value");
            }
        }
        else
        {
            Length = null;
        }

        var transferEncoding = headers.GetEntry(KnownHeaders.TransferEncoding);

        var chunked = transferEncoding != null && transferEncoding.Value.Span.SequenceEqual(ChunkedValue.Span);

        if (Length == null && !chunked)
        {
            throw new InvalidOperationException("Request body has neither Content-Length nor Transfer-Encoding: chunked");
        }
    }

    public void Reset()
    {
        _strategy = null;
        _reader = null;
        
        _streamStrategy.Reset();
        _memoryStrategy.Reset();
    }

    #endregion

    #region Functionality

    public Stream AsStream()
    {
        if (_strategy == ConsumptionStrategy.Stream)
        {
            return _streamStrategy.Obtain();
        }
        
        SetStrategy(ConsumptionStrategy.Stream);
        
        _streamStrategy.Apply(Reader, (long?)Length);
        
        return _streamStrategy.Obtain();
    }

    public ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync()
    {
        if (_strategy == ConsumptionStrategy.Memory)
        {
            return _memoryStrategy.ObtainAsync();
        }
        
        SetStrategy(ConsumptionStrategy.Memory);
        
        _memoryStrategy.Apply(Reader, (long?)Length);

        return _memoryStrategy.ObtainAsync();
    }

    private void SetStrategy(ConsumptionStrategy requested)
    {
        if (_strategy != null)
        {
            throw new InvalidOperationException($"Cannot provide body as '{requested}' as it has already been opened as '{_strategy}'");
        }

        _strategy = requested;
    }

    public ValueTask DrainAsync()
    {
        var strategy = _strategy;

        _strategy = null;
        
        if (strategy == ConsumptionStrategy.Stream)
        {
            return _streamStrategy.DrainAsync();
        }
        
        if (strategy == ConsumptionStrategy.Memory)
        {
            return _memoryStrategy.DrainAsync();
        }

        return default;
    }

    #endregion

}
