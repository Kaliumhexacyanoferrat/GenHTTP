using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using GenHTTP.Api.Caching;
using GenHTTP.Api.Http;
using GenHTTP.Api.Compression;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Represents a file download.
    /// </summary>
    public class Download
    {
        private string _File;
        private FileCache _Cache;
        private uint _Limit = 0;
        private long _UncompressedLength;
        private ContentType _ContentType;
        private DateTime _LastModified;
        
        public string File => _File;

        public FileCache Cache => _Cache;

        public uint Limit => _Limit;

        public DateTime LastModified => _LastModified;

        /// <summary>
        /// Create a new download.
        /// </summary>
        /// <param name="file">The file to send to the client</param>
        public Download(string file)
        {
            _File = file;
            GetContentType();
        }

        /// <summary>
        /// Create a new download.
        /// </summary>
        /// <param name="file">The file to send to the client</param>
        /// <param name="limit">The maximum send speed (bytes per seconds)</param>
        public Download(string file, uint limit) : this(file)
        {
            _Limit = limit;
        }

        /// <summary>
        /// Create a new download.
        /// </summary>
        /// <param name="file">The file to send to the client</param>
        /// <param name="cache">The file to use</param>
        public Download(string file, FileCache cache)
        {
            _File = file;
            _Cache = cache;
            GetContentType();
        }

        /// <summary>
        /// Create a new download.
        /// </summary>
        /// <param name="file">The file to send to the client</param>
        /// <param name="cache">The file cache to use</param>
        /// <param name="limit">The maximum send speed (bytes per second)</param>
        public Download(string file, FileCache cache, uint limit) : this(file, cache)
        {
            _Limit = limit;
        }

        private void GetContentType()
        {
            FileInfo info = new FileInfo(_File);
            _ContentType = Mapping.GetContentTypeByExtension(info.Extension.Substring(1));
            _LastModified = info.LastWriteTime;
            _UncompressedLength = info.Length;
        }
        
        /// <summary>
        /// The uncompressed length of the download.
        /// </summary>
        public long UncompressedLength
        {
            get
            {
                try
                {
                    if (_Cache != null)
                    {
                        if (_Cache.Load(_File)) return _Cache.Content(_File).LongLength;
                    }
                    return _UncompressedLength;
                }
                catch { return 0; }
            }
        }

        /// <summary>
        /// The type of this download.
        /// </summary>
        public ContentType ContentType
        {
            get
            {
                return _ContentType;
            }
            set
            {
                _ContentType = value;
            }
        }

        /// <summary>
        /// Retrieve the content of the file.
        /// </summary>
        /// <returns>The content of the file</returns>
        public byte[] GetBytes()
        {
            FileStream stream = new FileStream(_File, FileMode.Open, FileAccess.Read);
            byte[] ret = new byte[stream.Length];
            stream.Read(ret, 0, ret.Length);
            stream.Close();
            return ret;
        }
        
    }

}
