using System;
using System.IO;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.FileSystem
{

    public class FileResource : IResource
    {
        private ulong _Length;

        #region Get-/Setters

        public FileInfo File { get; }

        public string? Name { get; }

        public DateTime? Modified { get; private set; }

        public FlexibleContentType? ContentType { get; }

        public ulong? Length => _Length;

        #endregion

        #region Initialization

        public FileResource(FileInfo file, string? name, FlexibleContentType? contentType)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException("File does not exist", file.FullName);
            }

            File = file;

            Name = name ?? file.Name;

            _Length = (ulong)file.Length;
            Modified = file.LastWriteTimeUtc;

            ContentType = contentType ?? new FlexibleContentType(Name.GuessContentType() ?? Api.Protocol.ContentType.ApplicationForceDownload);
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

                Refresh();

                hash = hash * 23 + (ulong)Modified.GetHashCode();
                hash = hash * 23 + _Length;

                return hash;
            }
        }

        private void Refresh()
        {
            File.Refresh();

            _Length = (ulong)File.Length;
            Modified = File.LastWriteTimeUtc;
        }

        #endregion

    }

}
