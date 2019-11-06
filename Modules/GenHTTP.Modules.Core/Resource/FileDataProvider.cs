using System.IO;

using GenHTTP.Api.Modules;

namespace GenHTTP.Modules.Core.Resource
{

    public class FileDataProvider : IResourceProvider
    {

        #region Get-/Setters

        public FileInfo File { get; }

        public bool AllowCache => false;

        #endregion

        #region Initialization

        public FileDataProvider(FileInfo file)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException("Template file does not exist", file.FullName);
            }

            File = file;
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
