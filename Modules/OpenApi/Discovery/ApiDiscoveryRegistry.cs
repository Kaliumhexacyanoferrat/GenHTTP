using GenHTTP.Api.Content;
using GenHTTP.Modules.OpenApi.Handler;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class ApiDiscoveryRegistry
{

    #region Initialization

    public ApiDiscoveryRegistry(List<IApiExplorer> explorers)
    {
        Explorers = explorers;
    }

    #endregion

    #region Get-/Setters

    public List<IApiExplorer> Explorers { get; }

    #endregion

    #region Functionality

    internal void Explore(IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata)
    {
        foreach (var explorer in Explorers)
        {
            if (explorer.CanExplore(handler))
            {
                explorer.Explore(handler, path, document, schemata, this);
            }
        }
    }

    #endregion

}
