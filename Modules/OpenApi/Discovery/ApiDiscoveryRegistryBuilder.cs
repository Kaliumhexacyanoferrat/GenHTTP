using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class ApiDiscoveryRegistryBuilder : IBuilder<ApiDiscoveryRegistry>
{
    private readonly List<IApiExplorer> _Explorers = [];

    #region Functionality

    public ApiDiscoveryRegistryBuilder Add<TExplorer>() where TExplorer : IApiExplorer, new()
    {
        _Explorers.Add(new TExplorer());
        return this;
    }

    public ApiDiscoveryRegistryBuilder Add(IApiExplorer explorer)
    {
        _Explorers.Add(explorer);
        return this;
    }

    public ApiDiscoveryRegistry Build()
    {
        return new(_Explorers);
    }

    #endregion

}
