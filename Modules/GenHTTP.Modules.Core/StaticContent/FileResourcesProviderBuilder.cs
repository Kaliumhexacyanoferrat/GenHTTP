using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class FileResourcesProviderBuilder : IRouterBuilder
    {
        private DirectoryInfo? _Directory;
        
        #region Functionality
                
        public FileResourcesProviderBuilder Directory(DirectoryInfo directory)
        {
            _Directory = directory;
            return this;
        }

        public IRouter Build()
        {
            if (_Directory == null)
            {
                throw new BuilderMissingPropertyException("Directory");
            }

            if (!_Directory.Exists)
            {
                throw new DirectoryNotFoundException($"The given directory does not exist: '{_Directory.FullName}'");
            }
            
            return new FileResourcesProvider(_Directory);
        }

        #endregion

    }

}
