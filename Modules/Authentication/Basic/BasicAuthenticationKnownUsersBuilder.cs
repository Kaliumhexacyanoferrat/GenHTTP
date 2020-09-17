﻿using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Authentication.Basic
{

    public class BasicAuthenticationKnownUsersBuilder : IConcernBuilder
    {
        private readonly Dictionary<string, string> _Users = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
                        return new BasicAuthenticationUser(user);
                    }
                }

                return null;
            });
        }

        #endregion

    }

}
