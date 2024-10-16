using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Converts the result fetched from an invocation using reflection
/// into a HTTP response.
/// </summary>
public class ResponseProvider
{

    #region Get-/Setters

    private MethodRegistry Registry { get; }

    #endregion

    #region Initialization

    public ResponseProvider(MethodRegistry registry)
    {
        Registry = registry;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> GetResponseAsync(IRequest request, IHandler handler, object? result, Action<IResponseBuilder>? adjustments = null)
    {
        // no result = 204
        if (result is null)
        {
            return request.Respond()
                          .Status(ResponseStatus.NoContent)
                          .Adjust(adjustments)
                          .Build();
        }

        var type = result.GetType();

        // unwrap the result if applicable
        if (typeof(IResultWrapper).IsAssignableFrom(type))
        {
            var wrapped = (IResultWrapper)result;

            return await GetResponseAsync(request, handler, wrapped.Payload, b => wrapped.Apply(b));
        }

        // response returned by the method
        if (result is IResponseBuilder responseBuilder)
        {
            return responseBuilder.Adjust(adjustments).Build();
        }

        if (result is IResponse response)
        {
            return response;
        }

        // handler returned by the method
        if (result is IHandlerBuilder handlerBuilder)
        {
            return await handlerBuilder.Build(handler)
                                       .HandleAsync(request);
        }

        if (result is IHandler resultHandler)
        {
            return await resultHandler.HandleAsync(request);
        }

        // stream returned as a download
        if (result is Stream download)
        {
            var downloadResponse = request.Respond()
                                          .Content(download, () => download.CalculateChecksumAsync())
                                          .Type(ContentType.ApplicationForceDownload)
                                          .Adjust(adjustments)
                                          .Build();

            return downloadResponse;
        }

        // format the value if possible
        if (Registry.Formatting.CanHandle(type))
        {
            return request.Respond()
                          .Content(Registry.Formatting.Write(result, type) ?? string.Empty)
                          .Type(ContentType.TextPlain)
                          .Adjust(adjustments)
                          .Build();
        }

        // serialize the result
        var serializer = Registry.Serialization.GetSerialization(request);

        if (serializer is null)
        {
            throw new ProviderException(ResponseStatus.UnsupportedMediaType, "Requested format is not supported");
        }

        var serializedResult = await serializer.SerializeAsync(request, result);

        return serializedResult.Adjust(adjustments)
                               .Build();
    }

    #endregion

}
