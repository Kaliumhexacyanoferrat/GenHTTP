using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Authentication.Web.Controllers;
using GenHTTP.Modules.Controllers;
using System;

namespace GenHTTP.Modules.Authentication.Web.Concern
{

    public sealed class WebAuthenticationBuilder : IConcernBuilder
    {
        private IWebAuthenticationBackend? _Backend;

        private IHandlerBuilder? _SetupHandler;
        private string? _SetupRoute;

        #region Functionality

        public WebAuthenticationBuilder Backend(IWebAuthenticationBackend backend)
        {
            _Backend = backend;
            return this;
        }

        public WebAuthenticationBuilder Backend<T>() where T : IWebAuthenticationBackend, new() => Backend(new T());

        public WebAuthenticationBuilder EnableSetup(IHandlerBuilder? handler = null, string? route = null)
        {
            _SetupHandler = handler ?? Controller.From<SetupController>();
            _SetupRoute = route ?? "setup";

            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            var backend = _Backend ?? throw new BuilderMissingPropertyException("Backend");

            return new WebAuthenticationConcern(parent, contentFactory, backend, _SetupHandler, _SetupRoute);
        }

        #endregion

    }

}
