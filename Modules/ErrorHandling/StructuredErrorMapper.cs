using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Json;

namespace GenHTTP.Modules.ErrorHandling;

public sealed class StructuredErrorMapper : IErrorMapper<Exception>
{

    #region Supporting data structures

    public record ErrorModel(ResponseStatus Status, string Message, string? StackTrace = null);

    #endregion

    #region Get-/Setters

    public SerializationRegistry Registry { get; }

    #endregion

    #region Initialization

    public StructuredErrorMapper(SerializationRegistry? registry)
    {
        Registry = registry ?? Serialization.Default().Build();
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
    {
        string? stackTrace = null;

        if (request.Server.Development)
        {
            stackTrace = error.StackTrace;
        }

        if (error is ProviderException e)
        {
            var model = new ErrorModel(e.Status, error.Message, stackTrace);

            return await RenderAsync(request, model);
        }
        else
        {
            var model = new ErrorModel(ResponseStatus.InternalServerError, error.Message, stackTrace);

            return await RenderAsync(request, model);
        }
    }

    public async ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler)
    {
        var model = new ErrorModel(ResponseStatus.NotFound, "The requested resource does not exist on this server");

        return await RenderAsync(request, model);
    }

    private async ValueTask<IResponse> RenderAsync(IRequest request, ErrorModel model)
    {
        var format = Registry.GetSerialization(request) ?? new JsonFormat();

        var response = await format.SerializeAsync(request, model);

        response.Status(model.Status);

        return response.Build();
    }

    #endregion

}
