using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class EmbeddedResourcesProviderBuilder : RouterBuilderBase<EmbeddedResourcesProviderBuilder>
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

        public override IRouter Build()
        {
            if (_Root == null)
            {
                throw new BuilderMissingPropertyException("Root");
            }

            if (_Assembly == null)
            {
                _Assembly = System.Reflection.Assembly.GetCallingAssembly();
            }

            return new EmbeddedResourcesProvider(_Assembly, _Root, _Template, _ErrorHandler);
        }

        #endregion

    }

}
