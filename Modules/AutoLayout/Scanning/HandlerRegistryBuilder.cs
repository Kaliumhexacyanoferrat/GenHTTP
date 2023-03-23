using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.AutoLayout.Scanning
{

    public class HandlerRegistryBuilder : IBuilder<HandlerRegistry>
    {
        private readonly List<IResourceHandlerProvider> _Providers = new();

        private IResourceHandlerProvider? _Fallback;

        #region Functionality

        public HandlerRegistryBuilder Add(IResourceHandlerProvider provider)
        {
            _Providers.Add(provider);
            return this;
        }

        public HandlerRegistryBuilder Fallback(IResourceHandlerProvider provider)
        {
            _Fallback = provider;
            return this;
        }

        public HandlerRegistry Build()
        {
            return new HandlerRegistry(_Providers, _Fallback ?? throw new BuilderMissingPropertyException("fallback"));
        }

        #endregion

    }

}
