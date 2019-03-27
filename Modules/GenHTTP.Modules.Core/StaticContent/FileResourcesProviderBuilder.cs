using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class FileResourcesProviderBuilder : IBuilder<FileResourcesProvider>
    {
        private DirectoryInfo? _Directory;
        
        #region Functionality
                
        public FileResourcesProviderBuilder Directory(DirectoryInfo directory)
        {
            _Directory = directory;
            return this;
        }

        public FileResourcesProvider Build()
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
