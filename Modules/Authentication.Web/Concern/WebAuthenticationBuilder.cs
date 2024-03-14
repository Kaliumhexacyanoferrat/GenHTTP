using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using System;

namespace GenHTTP.Modules.Authentication.Web.Concern
{

    public sealed class WebAuthenticationBuilder : IConcernBuilder
    {
        private bool _AllowAnonymous;

        private SessionConfig? _SessionConfig;

        private SetupConfig? _SetupConfig;

        #region Functionality

        public WebAuthenticationBuilder AllowAnonymous()
        {
            _AllowAnonymous = true;
            return this;
        }

        public WebAuthenticationBuilder SessionHandling(SessionConfig sessionConfig)
        {
            _SessionConfig = sessionConfig;
            return this;
        }

        public WebAuthenticationBuilder EnableSetup(SetupConfig setupConfig)
        {
            _SetupConfig = setupConfig;
            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            var sessionConfig = _SessionConfig ?? throw new BuilderMissingPropertyException("Sessions");

            return new WebAuthenticationConcern(parent, contentFactory, _AllowAnonymous, sessionConfig, _SetupConfig);
        }

        #endregion

    }

}
