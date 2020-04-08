using System.IO;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class FileResourcesProviderBuilder : IHandlerBuilder
    {
        private DirectoryInfo? _Directory;

        #region Functionality

        public FileResourcesProviderBuilder Directory(DirectoryInfo directory)
        {
            _Directory = directory;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_Directory == null)
            {
                throw new BuilderMissingPropertyException("Directory");
            }

            if (!_Directory.Exists)
            {
                throw new DirectoryNotFoundException($"The given directory does not exist: '{_Directory.FullName}'");
            }

            return new FileResourcesProvider(parent, _Directory);
        }

        #endregion

    }

}
