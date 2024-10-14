using GenHTTP.Modules.OpenApi.Handler;

using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi;

public static class ApiDescription
{

    public static OpenApiHandlerBuilder Create() => new();

    public static OpenApiHandlerBuilder From(OpenApiDocument document) => new(document);

}
