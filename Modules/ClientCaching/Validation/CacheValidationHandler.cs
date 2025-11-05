using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.ClientCaching.Validation;

public sealed class CacheValidationHandler : IConcern
{
    private const string EtagHeader = "ETag";

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

        if (request.HasType(SupportedMethods))
        {
            if (response?.Content != null)
            {
                var eTag = await CalculateETag(response);

                var cached = request["If-None-Match"];

                if (cached is not null && cached == eTag)
                {
                    response.Status = new FlexibleResponseStatus(ResponseStatus.NotModified);

                    response.Content = null;

                    response.ContentEncoding = null;
                    response.ContentLength = null;
                    response.ContentType = null;
                }

                if (eTag is not null)
                {
                    response.Headers[EtagHeader] = eTag;
                }
            }
        }

        return response;
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    private static async ValueTask<string?> CalculateETag(IResponse response)
    {
        if (response.Headers.TryGetValue(EtagHeader, out var eTag))
        {
            return eTag;
        }

        if (response.Content is not null)
        {
            var checksum = await response.Content.CalculateChecksumAsync();

            if (checksum is not null)
            {
                return $"\"{checksum}\"";
            }
        }

        return null;
    }

    #endregion

}
