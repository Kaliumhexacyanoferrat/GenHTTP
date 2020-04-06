using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class EmbeddedResourcesProviderBuilder : IHandlerBuilder
    {
        private Assembly? _Assembly;
        private string? _Root;

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

        public IHandler Build(IHandler parent)
        {
            if (_Root == null)
            {
                throw new BuilderMissingPropertyException("Root");
            }

            if (_Assembly == null)
            {
                _Assembly = System.Reflection.Assembly.GetCallingAssembly();
            }

            return new EmbeddedResourcesProvider(parent, _Assembly, _Root);
        }

        #endregion

    }

}
