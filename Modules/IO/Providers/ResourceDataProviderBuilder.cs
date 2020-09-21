using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;

namespace GenHTTP.Modules.IO.Providers
{

    public class ResourceDataProviderBuilder : IBuilder<IResourceProvider>
    {
        private Assembly? _Assembly;
        private string? _Name;

        #region Functionality

        public ResourceDataProviderBuilder Assembly(Assembly assembly)
        {
            _Assembly = assembly;
            return this;
        }

        public ResourceDataProviderBuilder Name(string name)
        {
            _Name = name;
            return this;
        }

        public IResourceProvider Build()
        {
            if (_Name == null)
            {
                throw new BuilderMissingPropertyException("Name");
            }

            if (_Assembly == null)
            {
                _Assembly = System.Reflection.Assembly.GetCallingAssembly();
            }

            return new ResourceDataProvider(_Assembly, _Name);
        }

        #endregion

    }

}
