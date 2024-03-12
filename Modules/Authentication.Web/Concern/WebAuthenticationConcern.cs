using GenHTTP.Api.Content;
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

        private SetupConfig? SetupConfig { get; }

        private IHandler? SetupHandler { get; }

        #endregion

        #region Initialization

        public WebAuthenticationConcern(IHandler parent, Func<IHandler, IHandler> contentFactory,
                                        SetupConfig? setupConfig)
        {
            Parent = parent;
            Content = contentFactory(this);

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
            }

            return await Content.HandleAsync(request);
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
