using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Authentication.Basic
{

    public class BasicAuthenticationConcernBuilder : IConcernBuilder
    {
        private string? _Realm;

        private Func<string, string, ValueTask<IUser?>>? _Handler;

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

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            var realm = _Realm ?? throw new BuilderMissingPropertyException("Realm");

            var handler = _Handler ?? throw new BuilderMissingPropertyException("Handler");

            return new BasicAuthenticationConcern(parent, contentFactory, realm, handler);
        }

        #endregion

    }

}
