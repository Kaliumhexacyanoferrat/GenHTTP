using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.ClientCaching.Validation;

public sealed class CacheValidationHandler : IDocumentedConcern
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

        var cached = request.Header.Headers.GetEntry(KnownHeaders.IfNoneMatch).PreAllocate(request);

        var response = await Content.HandleAsync(request);

        if (response != null && isSupported)
        {
            if ((response.Content != null) && (response.Mode != Connection.Upgrade))
            {
                var existing = response.Headers.GetEntry(KnownHeaders.ETag);

                var eTag = existing ?? await CalculateETag(response);

                var builder = response.Rebuild();

                if (cached is not null && eTag is not null)
                {
                    if (cached.Value == eTag.Value)
                    {
                        builder.Status(ResponseStatus.NotModified);
                        builder.Content(null);
                    }
                }

                if (existing is null && eTag is not null)
                {
                    builder.Header(KnownHeaders.ETag, eTag.Value);
                }
            }
        }

        return response;
    }

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    public void AddDocumentation(OperationDocumentation operation)
    {
        if (operation.Method == RequestMethod.Get || operation.Method == RequestMethod.Head)
        {
            operation.Add(new RequestHeaderAttribute("If-None-Match", "The ETag of a previously received response, to check whether the content has changed since"))
                     .Add(new ResponseAttribute(ResponseStatus.NotModified, "The content has not changed since it has been received with the given ETag"))
                     .Add(new ResponseHeaderAttribute(ResponseStatus.Ok, "ETag", "Identifies the version of the returned content"))
                     .Add(new ResponseHeaderAttribute(ResponseStatus.NotModified, "ETag", "Identifies the version of the content"));
        }
    }

    private static async ValueTask<ByteString?> CalculateETag(IResponse response)
    {
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
