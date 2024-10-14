using GenHTTP.Api.Content;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class ApiDiscoveryRegistry
{

    #region Get-/Setters

    public List<IApiExplorer> Explorers { get; }

    #endregion

    #region Initialization

    public ApiDiscoveryRegistry(List<IApiExplorer> explorers)
    {
        Explorers = explorers;
    }

    #endregion

    #region Functionality

    internal void Explore(IHandler handler, List<string> path)
    {
        foreach (var explorer in Explorers)
        {
            if (explorer.CanExplore(handler))
            {
                explorer.Explore(handler, path, this);
            }
        }
    }

    #endregion

}
