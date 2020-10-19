using System.IO;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.Providers
{

    public class FileDataProvider : IResource
    {

        #region Get-/Setters

        public FileInfo File { get; }

        #endregion

        #region Initialization

        public FileDataProvider(FileInfo file)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException("File does not exist", file.FullName);
            }

            File = file;
        }

        #endregion

        #region Functionality

        public Stream GetContent()
        {
            return File.OpenRead();
        }

        public ulong GetChecksum()
        {
            unchecked
            {
                ulong hash = 17;

                hash = hash * 23 + (ulong)File.LastWriteTimeUtc.GetHashCode();
                hash = hash * 23 + (ulong)File.Length;

                return hash;
            }
        }

        #endregion

    }

}
