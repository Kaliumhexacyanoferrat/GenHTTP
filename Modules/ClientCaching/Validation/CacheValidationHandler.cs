using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.ClientCaching.Validation;

public sealed class CacheValidationHandler : IConcern
{
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
        var isSupported = request.HasType(SupportedMethods);
        
        var cached = request.Header.Headers.GetEntry(KnownHeaders.IfNoneMatch);
        
        var response = await Content.HandleAsync(request);

        if (response != null && isSupported)
        {
            if ((response.Content != null) && (response.Mode != Connection.Upgrade))
            {
                var eTag = await CalculateETag(response);

                var builder = response.Rebuild();

                if (cached is not null && eTag is not null)
                {
                    if (cached.Value == eTag.Value)
                    {
                        builder.Status(ResponseStatus.NotModified);
                        builder.Content(null);
                    }
                }

                if (eTag is not null)
                {
                    builder.Header(KnownHeaders.ETag, eTag.Value);
                }
            }
        }

        return response;
    }

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    private static async ValueTask<ByteString?> CalculateETag(IResponse response)
    {
        var eTag = response.Headers.GetEntry(KnownHeaders.ETag);

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
