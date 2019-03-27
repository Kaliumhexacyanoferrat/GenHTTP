using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class EmbeddedResourcesProviderBuilder : IBuilder<EmbeddedResourcesProvider>
    {
        protected Assembly? _Assembly;
        protected string? _Root;

        #region Functionality

        public EmbeddedResourcesProviderBuilder Assembly(Assembly assembly)
        {
            _Assembly = assembly;
            return this;
        }

        public EmbeddedResourcesProviderBuilder Root(string root)
        {
            _Root = root;
            return this;
        }

        public EmbeddedResourcesProvider Build()
        {
            if (_Root == null)
            {
                throw new BuilderMissingPropertyException("Root");
            }

            if (_Assembly == null)
            {
                _Assembly = System.Reflection.Assembly.GetCallingAssembly();
            }

            return new EmbeddedResourcesProvider(_Assembly, _Root);
        }

        #endregion

    }

}
