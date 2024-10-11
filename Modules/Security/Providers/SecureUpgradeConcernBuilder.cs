using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Security.Providers;

public sealed class SecureUpgradeConcernBuilder : IConcernBuilder
{
    private SecureUpgrade _Mode = SecureUpgrade.Force;

    #region Functionality

    public SecureUpgradeConcernBuilder Mode(SecureUpgrade mode)
    {
        _Mode = mode;
        return this;
    }

    public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory) => new SecureUpgradeConcern(parent, contentFactory, _Mode);

    #endregion

}
