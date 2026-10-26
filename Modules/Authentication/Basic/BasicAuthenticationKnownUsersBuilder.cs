using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Authentication.Documentation;

namespace GenHTTP.Modules.Authentication.Basic;

public sealed class BasicAuthenticationKnownUsersBuilder : IConcernBuilder
{
    private readonly SchemeDocumentation _documentation = new();

    private readonly Dictionary<string, string> _users = new(StringComparer.OrdinalIgnoreCase);

    private string? _realm;

    #region Functionality

    public BasicAuthenticationKnownUsersBuilder Realm(string realm)
    {
        _realm = realm;
        return this;
    }

    public BasicAuthenticationKnownUsersBuilder Add(string user, string password)
    {
        _users.Add(user, password);
        return this;
    }

    /// <summary>
    /// Describes how clients obtain their credentials, shown in a generated API specification.
    /// </summary>
    /// <param name="description">The description to be shown to the consumers of the API (may contain Markdown)</param>
    public BasicAuthenticationKnownUsersBuilder Documentation(string description)
    {
        _documentation.Description = description;
        return this;
    }

    /// <summary>
    /// Documents the authentication mechanism as the given scheme, instead of HTTP basic authentication.
    /// </summary>
    /// <param name="scheme">The scheme to be shown to the consumers of the API</param>
    public BasicAuthenticationKnownUsersBuilder Documentation(SecurityScheme scheme)
    {
        _documentation.Scheme = scheme;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var realm = _realm ?? throw new BuilderMissingPropertyException("Realm");

        return new BasicAuthenticationConcern(content, realm, (user, password) =>
        {
            if (_users.TryGetValue(user, out var expected))
            {
                if (password == expected)
                {
                    return new ValueTask<IUser?>(new BasicAuthenticationUser(user));
                }
            }

            return new ValueTask<IUser?>();
        }, _documentation.Resolve(SecurityScheme.Basic()));
    }

    #endregion

}
