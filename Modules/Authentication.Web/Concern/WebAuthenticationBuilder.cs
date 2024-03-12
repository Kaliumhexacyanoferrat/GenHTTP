using GenHTTP.Api.Content;
using System;

namespace GenHTTP.Modules.Authentication.Web.Concern
{

    public sealed class WebAuthenticationBuilder : IConcernBuilder
    {
        private SetupConfig? _SetupConfig;

        #region Functionality

        public WebAuthenticationBuilder EnableSetup(SetupConfig setupConfig)
        {
            _SetupConfig = setupConfig;
            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            return new WebAuthenticationConcern(parent, contentFactory, _SetupConfig);
        }

        #endregion

    }

}
