using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Basics;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
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

        private bool AllowAnonymous { get; }

        private SessionConfig SessionConfig { get; }

        private SetupConfig? SetupConfig { get; }

        private IHandler? SetupHandler { get; }

        #endregion

        #region Initialization

        public WebAuthenticationConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, bool allowAnonymous,
                                        SessionConfig sessionConfig, SetupConfig? setupConfig)
        {
            Parent = parent;
            Content = contentFactory(this);

            AllowAnonymous = allowAnonymous;
            SessionConfig = sessionConfig;

            SetupConfig = setupConfig;
            SetupHandler = setupConfig?.Handler.Build(this);
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
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
                    return await Content.HandleAsync(request);
                }
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

            // enforce login (todo)

            return null;
        }

        public void Append(PathBuilder path, IRequest request, IHandler? child = null)
        {
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

            if (segment == "{setup}")
            {
                return SetupHandler;
            }

            return null;
        }

        #endregion

    }

}
