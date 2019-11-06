using System.IO;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class FileResourcesProviderBuilder : RouterBuilderBase<FileResourcesProviderBuilder>
    {
        private DirectoryInfo? _Directory;

        #region Functionality

        public FileResourcesProviderBuilder Directory(DirectoryInfo directory)
        {
            _Directory = directory;
            return this;
        }

        public override IRouter Build()
        {
            if (_Directory == null)
            {
                throw new BuilderMissingPropertyException("Directory");
            }

            if (!_Directory.Exists)
            {
                throw new DirectoryNotFoundException($"The given directory does not exist: '{_Directory.FullName}'");
            }

            return new FileResourcesProvider(_Directory, _Template, _ErrorHandler);
        }

        #endregion

    }

}
