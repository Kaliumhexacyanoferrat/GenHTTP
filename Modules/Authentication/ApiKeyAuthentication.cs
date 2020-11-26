using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication.ApiKey;

namespace GenHTTP.Modules.Authentication
{
    
    public static class ApiKeyAuthentication
    {

        #region Builder

        public static ApiKeyConcernBuilder Create() => new ApiKeyConcernBuilder();

        #endregion

        #region Extensions

        public static T Authentication<T>(this T builder, ApiKeyConcernBuilder apiKeyAuth) where T : IHandlerBuilder<T>
        {
            builder.Add(apiKeyAuth);
            return builder;
        }

        public static T Authentication<T>(this T builder, Func<IRequest, string, ValueTask<IUser?>> authenticator) where T : IHandlerBuilder<T>
        {
            builder.Add(Create().Authenticator(authenticator));
            return builder;
        }

        #endregion

    }

}
