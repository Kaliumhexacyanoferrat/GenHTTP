using GenHTTP.Api.Content;
using GenHTTP.Modules.OpenApi.Discovery;

using NSwag;

namespace GenHTTP.Modules.OpenApi;

/// <summary>
/// Allows to analyze a specific kind of handler and adds the API endpoints defined
/// by the handler to the resulting OpenAPI document.
/// </summary>
public interface IApiExplorer
{

    /// <summary>
    /// Specfies, whether the given handler is supported by this explorer.
    /// </summary>
    /// <param name="handler">The handler to be inspected</param>
    /// <returns>true, if this explorer is capable of inspecting this explorer</returns>
    /// <remarks>
    /// Note that this method is only allowed to return true if the explorer can actually
    /// analyze this handler. No other explorer will be invoked for the handler in this case.
    /// </remarks>
    bool CanExplore(IHandler handler);

    /// <summary>
    /// Analyzes the given handler and adds the endpoints defined by this handler to the resulting
    /// OpenAPI document.
    /// </summary>
    /// <param name="handler">The handler to be analyzed</param>
    /// <param name="path">The current stack of path segments that have already been analyzed, relative to the location of the OpenAPI concern</param>
    /// <param name="document">The document to be adjusted and enriched</param>
    /// <param name="schemata">The manager to generate JSON schemas with</param>
    /// <param name="registry">The registry containing all active explorers which can be used to further analyze any child handler of the given handler instance</param>
    void Explore(IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry);

}
