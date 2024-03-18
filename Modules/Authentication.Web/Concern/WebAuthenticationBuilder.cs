using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Authentication.Web.Integration;
using System;

namespace GenHTTP.Modules.Authentication.Web.Concern
{

    public sealed class WebAuthenticationBuilder : IConcernBuilder
    {
        private IWebAuthIntegration? _Integration;

        private ISessionHandling? _SessionHandling;

        #region Functionality

        public WebAuthenticationBuilder Integration(IWebAuthIntegration integration)
        {
            _Integration = integration;
            return this;
        }

        public WebAuthenticationBuilder SessionHandling(ISessionHandling sessionHandling)
        {
            _SessionHandling = sessionHandling;
            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            var integration = _Integration ?? throw new BuilderMissingPropertyException("Integration");

            var sessionHandling = _SessionHandling ?? new DefaultSessionHandling();

            return new WebAuthenticationConcern(parent, contentFactory, integration, sessionHandling);
        }

        #endregion

    }

}
