using System.IO;

using GenHTTP.Api.Modules;

namespace GenHTTP.Modules.Core.Resource
{

    public class FileDataProvider : IResourceProvider
    {

        #region Get-/Setters

        public FileInfo File { get; }

        public bool AllowCache { get; private set; }

        #endregion

        #region Initialization

        public FileDataProvider(FileInfo file, bool allowCaching)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException("Template file does not exist", file.FullName);
            }

            File = file;
            AllowCache = allowCaching;
        }

        #endregion

        #region Functionality

        public Stream GetResource()
        {
            return File.OpenRead();
        }

        #endregion

    }

}
