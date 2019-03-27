using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Core.Resource
{

    public class ResourceDataProviderBuilder : IBuilder<ResourceDataProvider>
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

        public ResourceDataProvider Build()
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
