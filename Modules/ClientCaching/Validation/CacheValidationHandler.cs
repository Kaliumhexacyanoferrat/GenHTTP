using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.ClientCaching.Validation;

public sealed class CacheValidationHandler : IConcern
{
    private static readonly ReadOnlyMemory<byte> EtagHeader = "ETag"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> IfNoneMatchHeader = "If-None-Match"u8.ToArray();

    private static readonly RequestMethod[] SupportedMethods = [RequestMethod.Get, RequestMethod.Head];

    #region Get-/Setters

    public IHandler Content { get; }

    #endregion

    #region Initialization

    public CacheValidationHandler(IHandler content)
    {
        Content = content;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var response = await Content.HandleAsync(request);

        if (response != null && request.HasType(SupportedMethods))
        {
            if ((response.Content != null) && (response.Mode != Connection.Upgrade))
            {
                var eTag = await CalculateETag(response);

                var cached = request.Header.Headers.GetEntry(IfNoneMatchHeader);

                var builder = response.Rebuild();

                if (cached is not null && eTag is not null)
                {
                    if (cached.Value.Span.SequenceEqual(eTag.Value.Span))
                    {
                        builder.Status(ResponseStatus.NotModified);
                        builder.Content(null);
                    }
                }

                if (eTag is not null)
                {
                    builder.Header(EtagHeader, eTag.Value);
                }
            }
        }

        return response;
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    private static async ValueTask<ReadOnlyMemory<byte>?> CalculateETag(IResponse response)
    {
        var eTag = response.Headers.GetEntry(EtagHeader);

        if (eTag != null)
        {
            return eTag;
        }

        if (response.Content is not null)
        {
            ulong? checksum = await response.Content.CalculateChecksumAsync();

            if (checksum is not null)
            {
                Span<byte> buffer = stackalloc byte[22];

                buffer[0] = (byte)'"';

                checksum.Value.TryFormat(buffer[1..], out var written);

                buffer[written + 1] = (byte)'"';

                return new(buffer[..(written + 2)].ToArray());
            }
        }

        return null;
    }

    #endregion

}
