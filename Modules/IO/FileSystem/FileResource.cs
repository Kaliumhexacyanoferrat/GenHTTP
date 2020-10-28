using System;
using System.IO;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.FileSystem
{

    public class FileResource : IResource
    {

        #region Get-/Setters

        public FileInfo File { get; }

        public string? Name { get; }

        public DateTime? Modified { get; }

        public FlexibleContentType? ContentType { get; }

        #endregion

        #region Initialization

        public FileResource(FileInfo file, string? name, FlexibleContentType? contentType, DateTime? modified)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException("File does not exist", file.FullName);
            }

            File = file;

            Name = name ?? file.Name;
            Modified = modified ?? file.LastWriteTimeUtc;

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

                hash = hash * 23 + (ulong)File.LastWriteTimeUtc.GetHashCode();
                hash = hash * 23 + (ulong)File.Length;

                return hash;
            }
        }

        #endregion

    }

}
