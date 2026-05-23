using System.IO.Pipelines;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types.Body;

namespace GenHTTP.Engine.Shared.Types;

public class RequestBody : IRequestBody
{
    private static readonly ReadOnlyMemory<byte> ChunkedValue = "chunked"u8.ToArray();
    
    private readonly PipeReader _reader;

    private readonly bool _isChunked;

    private Stream? _stream;

    #region Get-/Setters

    public ContentType? Type { get; }

    public ReadOnlyMemory<byte>? Encoding { get; }

    public ulong? Length { get; }

    #endregion

    #region Initialization

    public RequestBody(IRequest request, PipeReader reader)
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

        var transferEncoding = headers.GetEntry(KnownHeaders.TransferEncoding);

        _isChunked = transferEncoding != null && transferEncoding.Value.Span.SequenceEqual(ChunkedValue.Span);
    }

    #endregion

    #region Functionality

    public Stream AsStream()
    {
        if (_stream != null)
        {
            return _stream;
        }

        if (Length != null)
        {
            return _stream = new LengthLimitedStream(_reader, (long)Length.Value);
        }

        if (_isChunked)
        {
            return _stream = new ChunkedBodyStream(_reader);
        }

        throw new InvalidOperationException("Request body has neither Content-Length nor Transfer-Encoding: chunked");
    }

    public ValueTask DrainAsync()
    {
        if (_stream is IDrainableStream drainable)
        {
            return drainable.DrainAsync();
        }

        return default;
    }

    #endregion

}
