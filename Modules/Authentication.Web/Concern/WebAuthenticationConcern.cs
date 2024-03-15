using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Authentication.Web.Concern
{

    public sealed class WebAuthenticationConcern : IConcern, IRootPathAppender, IHandlerResolver
    {

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        private bool AllowAnonymous { get; }

        private SessionConfig SessionConfig { get; }

        private LoginConfig LoginConfig { get; }

        private IHandler LoginHandler { get; }

        private SetupConfig? SetupConfig { get; }

        private IHandler? SetupHandler { get; }

        #endregion

        #region Initialization

        public WebAuthenticationConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, bool allowAnonymous,
                                        SessionConfig sessionConfig, LoginConfig loginConfig, SetupConfig? setupConfig)
        {
            Parent = parent;
            Content = contentFactory(this);

            AllowAnonymous = allowAnonymous;
            SessionConfig = sessionConfig;

            LoginConfig = loginConfig;
            LoginHandler = loginConfig.Handler.Build(this);

            SetupConfig = setupConfig;
            SetupHandler = setupConfig?.Handler.Build(this);
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            Login.SetConfig(request, LoginConfig);
            SessionHandling.SetConfig(request, SessionConfig);

            var segment = request.Target.Current;

            if ((SetupConfig != null) && (SetupHandler != null))
            {
                if (await SetupConfig.SetupRequired(request))
                {
                    if (segment?.Value != SetupConfig.Route)
                    {
                        // enforce setup wizard
                        return await Redirect.To("{setup}/", true)
                                             .Build(this)
                                             .HandleAsync(request);
                    }
                    else
                    {
                        request.Target.Advance();

                        Setup.SetConfig(request, SetupConfig);

                        return await SetupHandler.HandleAsync(request);
                    }
                }
                else if (segment?.Value == SetupConfig.Route)
                {
                    // do not allow setup to be called again
                    return await Redirect.To("{web-auth}", true)
                                         .Build(this)
                                         .HandleAsync(request);
                }
            }

            var token = await SessionConfig.ReadToken(request);

            if (token != null)
            {
                var authenticatedUser = await SessionConfig.VerifyToken(token);

                if (authenticatedUser != null)
                {
                    // we're logged in
                    request.SetUser(authenticatedUser);

                    // deny login and registration (todo)

                    var response = await Content.HandleAsync(request);

                    if (response != null)
                    {
                        // refresh the token, so the user will not be logged out eventually
                        SessionConfig.WriteToken(response, token);
                    }

                    return response;
                }
            }

            // handle login and registration (todo)
            if (segment?.Value == LoginConfig.Route)
            {
                request.Target.Advance();

                var loginResponse = await LoginHandler.HandleAsync(request);

                if (loginResponse != null)
                {
                    // establish the session if the user was authenticated
                    var authenticatedUser = request.GetUser<IUser>();

                    if (authenticatedUser != null)
                    {
                        var generatedToken = await SessionConfig.StartSession(request, authenticatedUser);

                        // actually tell the client about the token
                        SessionConfig.WriteToken(loginResponse, generatedToken);
                    }
                }

                return loginResponse;
            }

            if (AllowAnonymous)
            {
                var response = await Content.HandleAsync(request);

                if ((response != null) && (token != null))
                {
                    // clear the invalid cookie
                    SessionConfig.ClearToken(response);
                }

                return null;
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
                path.Preprend(LoginConfig.Route);
            }

            if (SetupConfig != null)
            {
                if (child == SetupHandler)
                {
                    path.Preprend(SetupConfig.Route);
                }
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

            return null;
        }

        #endregion

    }

}
