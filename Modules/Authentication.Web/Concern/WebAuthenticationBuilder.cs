using System;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Authentication.Web.Concern
{

    public sealed class WebAuthenticationBuilder<TUser> : IConcernBuilder where TUser : class, IUser
    {
        private readonly IWebAuthIntegration<TUser> _Integration;

        private ISessionHandling? _SessionHandling;

        #region Functionality

        public WebAuthenticationBuilder(IWebAuthIntegration<TUser> integration)
        {
            _Integration = integration;
        }

        /// <summary>
        /// Configures the session handline to be used by this concern.
        /// </summary>
        /// <typeparam name="T">The class used to implement session handling</typeparam>
        public WebAuthenticationBuilder<TUser> SessionHandling<T>() where T : ISessionHandling, new() => SessionHandling(new T());

        /// <summary>
        /// Configures the session handline instance to be used by this concern.
        /// </summary>
        /// <param name="sessionHandling">The session handling instance to be used</param>
        public WebAuthenticationBuilder<TUser> SessionHandling(ISessionHandling sessionHandling)
        {
            _SessionHandling = sessionHandling;
            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            var sessionHandling = _SessionHandling ?? throw new BuilderMissingPropertyException("Session Handling");

            return new WebAuthenticationConcern<TUser>(parent, contentFactory, _Integration, sessionHandling);
        }

        #endregion

    }

}
