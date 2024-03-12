using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenHTTP.Modules.Authentication.Web.Concern
{

    public sealed class WebAuthenticationConcern : IConcern, IRootPathAppender, IHandlerResolver
    {

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        public IWebAuthenticationBackend Backend { get; }

        //public ISessionNegotiation SessionNegotiation { get; }

        //public string LoginRoute { get; }

        //public bool AllowAnonymous { get; }

        //public string LogoutRoute { get; }

        //public string? RegistrationRoute { get; }

        public IHandler? SetupHandler { get; }

        public string? SetupRoute { get; }

        #endregion

        #region Initialization

        public WebAuthenticationConcern(IHandler parent, Func<IHandler, IHandler> contentFactory,
                                        IWebAuthenticationBackend backend, //ISessionNegotiation sessionNegotiation,
                                        //IHandlerBuilder loginHandler, string loginRoute, bool allowAnonymous,
                                        //IHandlerBuilder logoutHandler, string logoutRoute,
                                        //IHandlerBuilder? registrationHandler, string? registrationRoute,
                                        IHandlerBuilder? setupHandler, string? setupRoute)
        {
            Parent = parent;
            Content = contentFactory(this);

            Backend = backend;
            //SessionNegotiation = sessionNegotiation;

            //LoginRoute = loginRoute;
            //AllowAnonymous = allowAnonymous;

            //LogoutRoute = logoutRoute;

            SetupRoute = setupRoute;
            SetupHandler = setupHandler?.Build(this);

            /*if ((registrationRoute != null) && (registrationHandler != null))
            {
                RegistrationRoute = registrationRoute;
                overlay.Add(registrationRoute, registrationHandler);
            }*/
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var segment = request.Target.Current;

            if ((SetupRoute != null) && (SetupHandler != null))
            {
                if (await Backend.CheckSetupRequired(request))
                {
                    if (segment?.Value != SetupRoute)
                    {
                        return await Redirect.To("{setup}/", true)
                                             .Build(this)
                                             .HandleAsync(request);
                    }
                    else
                    {
                        request.Target.Advance();

                        return await SetupHandler.HandleAsync(request);
                    }
                }
            }

            return await Content.HandleAsync(request);
        }

        public void Append(PathBuilder path, IRequest request, IHandler? child = null)
        {
            if ((child == SetupHandler) && (SetupRoute != null))
            {
                path.Preprend(SetupRoute);
            }
        }

        public IHandler? Find(string segment)
        {
            if (segment == "{setup}")
            {
                return SetupHandler;
            }

            return null;
        }

        #endregion

    }

}
