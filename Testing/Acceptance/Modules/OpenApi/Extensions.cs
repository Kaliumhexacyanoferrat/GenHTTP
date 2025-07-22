using System.Net;

using GenHTTP.Api.Content;

using GenHTTP.Modules.Functional.Provider;
using GenHTTP.Modules.OpenApi;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

internal static class Extensions
{

    internal static async Task<((string, IOpenApiPathItem), OpenApiOperation)> GetOperationAsync(TestEngine engine, InlineBuilder api)
    {
        var doc = (await api.Add(ApiDescription.Create()).GetOpenApiAsync(engine)).Document!;

        var path = doc.Paths.First();

        return ((path.Key, path.Value), path.Value.Operations?.First().Value!);
    }

    internal static async Task<ReadResult> AsOpenApiAsync(this HttpResponseMessage response)
    {
        await response.AssertStatusAsync(HttpStatusCode.OK);

        await using var content = await response.Content.ReadAsStreamAsync();

        var settings = new OpenApiReaderSettings();

        settings.AddYamlReader();

        return await OpenApiDocument.LoadAsync(content, settings: settings);
    }

    internal static async Task<ReadResult> GetOpenApiAsync(this IHandlerBuilder api, TestEngine engine, bool validate = true)
    {
        await using var host = await TestHost.RunAsync(api, engine: engine);

        using var response = await host.GetResponseAsync("/openapi.json");

        var result = await response.AsOpenApiAsync();

        if (validate)
        {
            AssertX.Empty(result.Diagnostic?.Errors);
            AssertX.Empty(result.Diagnostic?.Warnings);
        }

        return result;
    }
}
