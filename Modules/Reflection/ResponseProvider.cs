﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Converts the result fetched from an invocation using reflection
/// into a HTTP response.
/// </summary>
public class ResponseProvider
{

    #region Initialization

    public ResponseProvider(MethodRegistry registry)
    {
        Registry = registry;
    }

    #endregion

    #region Get-/Setters

    private MethodRegistry Registry { get; }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> GetResponseAsync(IRequest request, IHandler handler, Operation operation, object? result, Action<IResponseBuilder>? adjustments = null)
    {
        // no result = 204
        if (result is null)
        {
            return GetNoContent(request, adjustments);
        }

        var type = result.GetType();

        // unwrap the result if applicable
        if (typeof(IResultWrapper).IsAssignableFrom(type))
        {
            var wrapped = (IResultWrapper)result;

            return await GetResponseAsync(request, handler, operation, wrapped.Payload, b => wrapped.Apply(b));
        }

        return operation.Result.Sink switch
        {
            OperationResultSink.Dynamic => await GetDynamicResponse(request, result, handler, adjustments),
            OperationResultSink.Stream => GetDownloadResponse(request, (Stream)result, adjustments),
            OperationResultSink.Formatter => GetFormattedResponse(request, result, type, adjustments),
            OperationResultSink.Serializer => await GetSerializedResponse(request, result, adjustments),
            OperationResultSink.None => GetNoContent(request, adjustments),
            _ => throw new ProviderException(ResponseStatus.InternalServerError, $"Unsupported sink '{operation.Result.Sink}' for type '{operation.Result.Type}'")
        };
    }

    private static IResponse GetNoContent(IRequest request, Action<IResponseBuilder>? adjustments) => request.Respond()
                                                                                                             .Status(ResponseStatus.NoContent)
                                                                                                             .Adjust(adjustments)
                                                                                                             .Build();

    private static async Task<IResponse?> GetDynamicResponse(IRequest request, object result, IHandler handler, Action<IResponseBuilder>? adjustments)
    {
        if (result is IResponseBuilder responseBuilder)
        {
            return responseBuilder.Adjust(adjustments).Build();
        }

        if (result is IResponse response)
        {
            return response;
        }

        if (result is IHandlerBuilder handlerBuilder)
        {
            return await handlerBuilder.Build(handler)
                                       .HandleAsync(request);
        }

        if (result is IHandler resultHandler)
        {
            return await resultHandler.HandleAsync(request);
        }

        throw new ProviderException(ResponseStatus.InternalServerError, $"Unexpected return type '{result.GetType()}' to be processed by dynamic sink");
    }

    private static IResponse GetDownloadResponse(IRequest request, Stream download, Action<IResponseBuilder>? adjustments)
    {
        var downloadResponse = request.Respond()
                                      .Content(download, download.CalculateChecksumAsync)
                                      .Type(ContentType.ApplicationForceDownload)
                                      .Adjust(adjustments)
                                      .Build();

        return downloadResponse;
    }

    private IResponse GetFormattedResponse(IRequest request, object result, Type type, Action<IResponseBuilder>? adjustments) => request.Respond()
                                                                                                                                        .Content(Registry.Formatting.Write(result, type) ?? string.Empty)
                                                                                                                                        .Type(ContentType.TextPlain)
                                                                                                                                        .Adjust(adjustments)
                                                                                                                                        .Build();

    private async ValueTask<IResponse> GetSerializedResponse(IRequest request, object result, Action<IResponseBuilder>? adjustments)
    {
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
