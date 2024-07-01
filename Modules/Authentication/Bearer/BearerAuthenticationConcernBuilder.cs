using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Bearer
{

    public sealed class BearerAuthenticationConcernBuilder : IConcernBuilder
    {
        private readonly TokenValidationOptions _Options = new();

        #region Functionality

        public BearerAuthenticationConcernBuilder Issuer(string issuer)
        {
            _Options.Issuer = issuer;
            return this;
        }

        public BearerAuthenticationConcernBuilder Audience(string audience)
        {
            _Options.Audience = audience;
            return this;
        }

        public BearerAuthenticationConcernBuilder Validation(Func<JwtSecurityToken, Task> validator)
        {
            _Options.CustomValidator = validator;
            return this;
        }

        public BearerAuthenticationConcernBuilder UserMapping(Func<IRequest, JwtSecurityToken, ValueTask<IUser?>> userMapping)
        {
            _Options.UserMapping = userMapping;
            return this;
        }

        public BearerAuthenticationConcernBuilder AllowExpired()
        {
            _Options.Lifetime = false;
            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            return new BearerAuthenticationConcern(parent, contentFactory, _Options);
        }

        #endregion

    }

}
