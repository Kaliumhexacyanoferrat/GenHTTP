using System.Collections.Generic;
using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.Providers
{

    public class EmbeddedResourcesProviderBuilder : IHandlerBuilder<EmbeddedResourcesProviderBuilder>
    {
        private Assembly? _Assembly;
        private string? _Root;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

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

        public EmbeddedResourcesProviderBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
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

            return Concerns.Chain(parent, _Concerns, (p) => new EmbeddedResourcesProvider(p, _Assembly, _Root));
        }

        #endregion

    }

}
