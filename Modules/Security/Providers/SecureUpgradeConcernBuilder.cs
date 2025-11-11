using GenHTTP.Api.Content;

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

    public IConcern Build(IHandler content) => new SecureUpgradeConcern(content, _Mode);

    #endregion

}
