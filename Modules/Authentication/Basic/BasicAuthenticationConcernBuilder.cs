using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Authentication.Basic;

public sealed class BasicAuthenticationConcernBuilder : IConcernBuilder
{

    private Func<string, string, ValueTask<IUser?>>? _Handler;
    private string? _Realm;

    #region Functionality

    public BasicAuthenticationConcernBuilder Realm(string realm)
    {
        _Realm = realm;
        return this;
    }

    public BasicAuthenticationConcernBuilder Handler(Func<string, string, ValueTask<IUser?>> handler)
    {
        _Handler = handler;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var realm = _Realm ?? throw new BuilderMissingPropertyException("Realm");

        var handler = _Handler ?? throw new BuilderMissingPropertyException("Handler");

        return new BasicAuthenticationConcern(content, realm, handler);
    }

    #endregion

}
