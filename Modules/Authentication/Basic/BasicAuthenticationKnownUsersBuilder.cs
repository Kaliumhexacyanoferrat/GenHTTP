using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Authentication.Basic;

public sealed class BasicAuthenticationKnownUsersBuilder : IConcernBuilder
{
    private readonly Dictionary<string, string> _Users = new(StringComparer.OrdinalIgnoreCase);

    private string? _Realm;

    #region Functionality

    public BasicAuthenticationKnownUsersBuilder Realm(string realm)
    {
            _Realm = realm;
            return this;
        }

    public BasicAuthenticationKnownUsersBuilder Add(string user, string password)
    {
            _Users.Add(user, password);
            return this;
        }

    public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
    {
            var realm = _Realm ?? throw new BuilderMissingPropertyException("Realm");

            return new BasicAuthenticationConcern(parent, contentFactory, realm, (user, password) =>
            {
                if (_Users.TryGetValue(user, out var expected))
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
