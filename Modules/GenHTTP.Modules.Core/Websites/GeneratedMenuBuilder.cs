using System;
using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Websites
{

    public class GeneratedMenuBuilder : IBuilder<IMenuProvider>
    {
        private Func<IRequest, IHandler, IEnumerable<ContentElement>>? _Provider;

        #region Functionality

        public GeneratedMenuBuilder Provider(Func<IRequest, IHandler, IEnumerable<ContentElement>> provider)
        {
            _Provider = provider;
            return this;
        }

        public IMenuProvider Build()
        {
            var provider = _Provider ?? throw new BuilderMissingPropertyException("provider");

            return new GeneratedMenuProvider(provider);
        }

        #endregion

    }

}
