using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types.Body;

namespace GenHTTP.Engine.Shared.Types;

public class RequestBody : IRequestBody
{
    private Stream _source;

    #region Get-/Setters

    public ContentType? Type { get; }

    public ReadOnlyMemory<byte>? Encoding { get; }

    public ulong? Length { get; }

    #endregion

    #region Initialization

    public RequestBody(IRequest request, Stream source)
    {
        _source = source;

        // request headers might be released so we need to persist the values here
        var headers = request.Header.Headers;

        var contentType = headers.GetEntry(KnownHeaders.ContentType);

        Type = (contentType != null) ? new(contentType.Value) : null;

        Encoding = headers.GetEntry(KnownHeaders.ContentEncoding);

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
    }

    #endregion

    #region Functionality

    public Stream AsStream()
    {
        if (Length != null)
        {
            return new LengthLimitedStream(_source, (long)Length.Value);
        }

        throw new NotImplementedException("Chunked encoding is not implemented yet");
    }

    #endregion

}
