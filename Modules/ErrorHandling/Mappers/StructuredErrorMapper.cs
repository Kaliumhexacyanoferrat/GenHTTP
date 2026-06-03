using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.ErrorHandling.Provider;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.ErrorHandling.Mappers;

public sealed class StructuredErrorMapper : IErrorMapper<Exception>
{

    #region Initialization

    public StructuredErrorMapper(SerializationRegistry? registry)
    {
        Registry = registry ?? GetDefaultSerialization();
    }

    private static SerializationRegistry GetDefaultSerialization()
    {
        var options = JsonFormat.GetDefaultOptions();

        // ensure error model can be serialized in AoT
        options.TypeInfoResolverChain.Add(ErrorHandlingContext.Default);

        return Serialization.Default(jsonOptions: options).Build();
    }

    #endregion

    #region Get-/Setters

    public SerializationRegistry Registry { get; }

    #endregion

    #region Supporting data structures

    public record ErrorModel(ResponseStatus Status, string Message, string? StackTrace = null);

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error, ByteString? acceptedFormat)
    {
        string? stackTrace = null;

        if (request.Server.Development)
        {
            stackTrace = error.StackTrace;
        }

        if (error is ProviderException e)
        {
            var model = new ErrorModel(e.Status, error.Message, stackTrace);

            return (await RenderAsync(request, model,acceptedFormat)).Apply(e.Modifications).Build();
        }
        else
        {
            var model = new ErrorModel(ResponseStatus.InternalServerError, error.Message, stackTrace);

            return (await RenderAsync(request, model, acceptedFormat)).Build();
        }
    }

    public async ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler, ByteString? acceptedFormat)
    {
        var model = new ErrorModel(ResponseStatus.NotFound, "The requested resource does not exist on this server");

        return (await RenderAsync(request, model, acceptedFormat)).Build();
    }

    private async ValueTask<IResponseBuilder> RenderAsync(IRequest request, ErrorModel model, ByteString? acceptedFormat)
    {
        var format = Registry.GetSerialization(request, acceptedFormat) ?? new JsonFormat();

        var response = await format.SerializeAsync(request, model);

        response.Status(model.Status);

        return response;
    }

    #endregion

}
