using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Documentation.Security;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Authentication.Documentation;

namespace GenHTTP.Modules.Authentication.Basic;

public sealed class BasicAuthenticationConcernBuilder : IAuthenticationBuilder<BasicAuthenticationConcernBuilder>
{
    private readonly SchemeDocumentation _documentation = new();

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

    public BasicAuthenticationConcernBuilder Documentation(string description)
    {
        _documentation.Description = description;
        return this;
    }

    public BasicAuthenticationConcernBuilder Documentation(SecurityScheme scheme)
    {
        _documentation.Scheme = scheme;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var realm = _realm ?? throw new BuilderMissingPropertyException("Realm");

        var handler = _handler ?? throw new BuilderMissingPropertyException("Handler");

        return new BasicAuthenticationConcern(content, realm, handler, _documentation.Resolve(SecurityScheme.Basic()));
    }

    #endregion

}
