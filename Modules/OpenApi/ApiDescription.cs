using GenHTTP.Modules.OpenApi.Handler;

using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi;

public static class ApiDescription
{

    public static OpenApiConcernBuilder Create() => new();

    public static OpenApiConcernBuilder From(OpenApiDocument document) => new(document);

}
