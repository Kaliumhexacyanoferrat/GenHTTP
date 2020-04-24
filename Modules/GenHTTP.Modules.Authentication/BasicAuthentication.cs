using System;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;

using GenHTTP.Modules.Authentication.Basic;

namespace GenHTTP.Modules.Authentication
{
    
    public static class BasicAuthentication
    {
        private const string DEFAULT_REALM = "Restricted Area";

        #region Builder

        public static BasicAuthenticationConcernBuilder Create(Func<string, string, IUser?> authenticator, string realm = DEFAULT_REALM)
        {
            return new BasicAuthenticationConcernBuilder().Handler(authenticator)
                                                          .Realm(realm);
        }

        public static BasicAuthenticationKnownUsersBuilder Create(string realm = DEFAULT_REALM)
        {
            return new BasicAuthenticationKnownUsersBuilder().Realm(realm);
        }

        #endregion

        #region Extensions

        public static T Authentication<T>(this T builder, Func<string, string, IUser?> authenticator, string realm = DEFAULT_REALM) where T : IHandlerBuilder<T>
        {
            builder.Add(Create(authenticator, realm));
            return builder;
        }

        public static T Authentication<T>(this T builder, BasicAuthenticationConcernBuilder basicAuth) where T : IHandlerBuilder<T>
        {
            builder.Add(basicAuth);
            return builder;
        }

        public static T Authentication<T>(this T builder, BasicAuthenticationKnownUsersBuilder basicAuth) where T : IHandlerBuilder<T>
        {
            builder.Add(basicAuth);
            return builder;
        }

        #endregion

    }

}
