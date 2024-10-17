using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class ApiDiscoveryRegistryBuilder : IBuilder<ApiDiscoveryRegistry>
{
    private readonly List<IApiExplorer> _Explorers = [];

    #region Functionality

    /// <summary>
    /// Adds the given explorer to the registry.
    /// </summary>
    /// <typeparam name="TExplorer">The type of the explorer to be added</typeparam>
    public ApiDiscoveryRegistryBuilder Add<TExplorer>() where TExplorer : IApiExplorer, new() => Add(new TExplorer());

    /// <summary>
    /// Adds the given explorer to the registry.
    /// </summary>
    /// <param name="explorer">The explorer to be added</param>
    public ApiDiscoveryRegistryBuilder Add(IApiExplorer explorer)
    {
        _Explorers.Add(explorer);
        return this;
    }

    public ApiDiscoveryRegistry Build() => new(_Explorers);

    #endregion

}
