using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.ApiKey
{

    public class ApiKeyConcernBuilder : IConcernBuilder
    {
        private Func<IRequest, string?>? _KeyExtractor = (request) => request.Headers.TryGetValue("X-API-Key", out var key) ? key : null;

        private Func<IRequest, string, ValueTask<IUser?>>? _Authenticator;

        #region Functionality

        public ApiKeyConcernBuilder Extractor(Func<IRequest, string?> keyExtractor)
        {
            _KeyExtractor = keyExtractor;
            return this;
        }

        public ApiKeyConcernBuilder WithHeader(string headerName)
        {
            _KeyExtractor = (request) => request.Headers.TryGetValue(headerName, out var key) ? key : null;
            return this;
        }

        public ApiKeyConcernBuilder WithQueryParameter(string parameter)
        {
            _KeyExtractor = (request) => request.Query.TryGetValue(parameter, out var key) ? key : null;
            return this;
        }

        public ApiKeyConcernBuilder Authenticator(Func<IRequest, string, ValueTask<IUser?>> authenticator)
        {
            _Authenticator = authenticator;
            return this;
        }

        public ApiKeyConcernBuilder Keys(params string[] allowedKeys)
        {
            var keySet = new HashSet<string>(allowedKeys);

            _Authenticator = (r, key) => keySet.Contains(key) ? new ValueTask<IUser?>(new ApiKeyUser(key)) : new ValueTask<IUser?>();

            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            var keyExtractor = _KeyExtractor ?? throw new BuilderMissingPropertyException("KeyExtractor");

            var authenticator = _Authenticator ?? throw new BuilderMissingPropertyException("Authenticator");

            return new ApiKeyConcern(parent, contentFactory, keyExtractor, authenticator);
        }

        #endregion

    }

}
