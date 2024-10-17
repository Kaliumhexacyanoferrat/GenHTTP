using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Modules.Functional.Provider;
using GenHTTP.Modules.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

internal static class Extensions
{

    internal static async Task<((string, OpenApiPathItem), OpenApiOperation)> GetOperationAsync(InlineBuilder api)
    {
        var doc = (await api.Add(ApiDescription.Create()).GetOpenApiAsync()).OpenApiDocument;

        var path = doc.Paths.First();

        return ((path.Key, path.Value), path.Value.Operations.First().Value);
    }

    internal static async Task<ReadResult> AsOpenApiAsync(this HttpResponseMessage response)
    {
        await response.AssertStatusAsync(HttpStatusCode.OK);

        await using var content = await response.Content.ReadAsStreamAsync();

        return await new OpenApiStreamReader().ReadAsync(content);
    }

    internal static async Task<ReadResult> GetOpenApiAsync(this IHandlerBuilder api, bool validate = true)
    {
        using var host = TestHost.Run(api);

        using var response = await host.GetResponseAsync("/openapi.json");

        var result = await response.AsOpenApiAsync();

        if (validate)
        {
            AssertX.Empty(result.OpenApiDiagnostic.Errors);
            AssertX.Empty(result.OpenApiDiagnostic.Warnings);
        }

        return result;
    }

}
