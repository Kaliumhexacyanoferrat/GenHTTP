using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Authentication.Basic;

public sealed class BasicAuthenticationKnownUsersBuilder : IConcernBuilder
{
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
        });
    }

    #endregion

}
