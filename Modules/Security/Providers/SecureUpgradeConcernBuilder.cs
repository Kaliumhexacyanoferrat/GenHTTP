using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Security.Providers;

public sealed class SecureUpgradeConcernBuilder : IConcernBuilder
{
    private SecureUpgrade _mode = SecureUpgrade.Force;

    #region Functionality

    public SecureUpgradeConcernBuilder Mode(SecureUpgrade mode)
    {
        _mode = mode;
        return this;
    }

    public IConcern Build(IHandler content) => new SecureUpgradeConcern(content, _mode);

    #endregion

}
