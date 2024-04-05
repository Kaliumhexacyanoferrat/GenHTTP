using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Basics;
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

        private IWebAuthIntegration Integration { get; }

        private ISessionHandling SessionHandling { get; }

        private IHandler LoginHandler { get; }

        private IHandler SetupHandler { get; }

        private IHandler ResourceHandler { get; }

        #endregion

        #region Initialization

        public WebAuthenticationConcern(IHandler parent, Func<IHandler, IHandler> contentFactory,
                                        IWebAuthIntegration integration, ISessionHandling sessionHandling)
        {
            Parent = parent;
            Content = contentFactory(this);

            Integration = integration;
            SessionHandling = sessionHandling;

            LoginHandler = integration.LoginHandler.Build(this);
            SetupHandler = integration.SetupHandler.Build(this);

            ResourceHandler = integration.ResourceHandler.Build(this);
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var segment = request.Target.Current;

            if (segment?.Value == Integration.ResourceRoute)
            {
                request.Target.Advance();

                return await ResourceHandler.HandleAsync(request).ConfigureAwait(false);
            }

            if (await Integration.CheckSetupRequired(request).ConfigureAwait(false))
            {
                if (segment?.Value != Integration.SetupRoute)
                {
                    // enforce setup wizard
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
            else if (segment?.Value == Integration.SetupRoute)
            {
                // do not allow setup to be called again
                return await Redirect.To("{web-auth}", true)
                                     .Build(this)
                                     .HandleAsync(request);
            }

            // try to fetch and validate the token
            var authenticated = false;

            var token = SessionHandling.ReadToken(request);

            if (token != null)
            {
                var authenticatedUser = await Integration.VerifyTokenAsync(token);

                if (authenticatedUser != null)
                {
                    // we're logged in
                    request.SetUser(authenticatedUser);

                    authenticated = true;
                }
            }

            // handle login
            if (segment?.Value == Integration.LoginRoute)
            {
                request.Target.Advance();

                var loginResponse = await LoginHandler.HandleAsync(request);

                if (loginResponse != null)
                {
                    // establish the session if the user was authenticated
                    var authenticatedUser = request.GetUser<IUser>();

                    if (authenticatedUser != null)
                    {
                        var generatedToken = await Integration.StartSessionAsync(request, authenticatedUser);

                        // actually tell the client about the token
                        SessionHandling.WriteToken(loginResponse, generatedToken);
                    }
                }

                return loginResponse;
            }

            if (authenticated)
            {
                var response = await Content.HandleAsync(request);

                if ((response != null) && (token != null))
                {
                    // refresh the token, so the user will not be logged out eventually
                    SessionHandling.WriteToken(response, token);
                }

                return response;
            }
            if (Integration.AllowAnonymous)
            {
                var response = await Content.HandleAsync(request);

                if ((response != null) && (token != null))
                {
                    // clear the invalid cookie
                    SessionHandling.ClearToken(response);
                }

                return response;
            }
            else
            {
                // enforce login
                return await Redirect.To("{login}/", true)
                                     .Build(this)
                                     .HandleAsync(request);
            }
        }

        public void Append(PathBuilder path, IRequest request, IHandler? child = null)
        {
            if (child == LoginHandler)
            {
                path.Preprend(Integration.LoginRoute);
            }

            if (child == SetupHandler)
            {
                path.Preprend(Integration.SetupRoute);
            }

            if (child == ResourceHandler)
            {
                path.Preprend(Integration.ResourceRoute);
            }
        }

        public IHandler? Find(string segment)
        {
            if (segment == "{web-auth}")
            {
                return this;
            }

            if (segment == "{login}")
            {
                return LoginHandler;
            }

            if (segment == "{setup}")
            {
                return SetupHandler;
            }

            if (segment == "{web-auth-resources}")
            {
                return ResourceHandler;
            }

            return null;
        }

        #endregion

    }

}
