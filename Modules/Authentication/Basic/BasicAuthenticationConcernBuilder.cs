using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Authentication.Basic;

public sealed class BasicAuthenticationConcernBuilder : IConcernBuilder
{

    private Func<string, string, ValueTask<IUser?>>? _handler;
    private string? _realm;

    #region Functionality

    public BasicAuthenticationConcernBuilder Realm(string realm)
    {
        _realm = realm;
        return this;
    }

    public BasicAuthenticationConcernBuilder Handler(Func<string, string, ValueTask<IUser?>> handler)
    {
        _handler = handler;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var realm = _realm ?? throw new BuilderMissingPropertyException("Realm");

        var handler = _handler ?? throw new BuilderMissingPropertyException("Handler");

        return new BasicAuthenticationConcern(content, realm, handler);
    }

    #endregion

}
