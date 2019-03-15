using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace GenHTTP
{

    /// <summary>
    /// Represents a file download.
    /// </summary>
    public class Download
    {
        private string _File;
        private BinaryReader r;
        private FileCache _Cache;
        private uint _Limit = 0;
        private long _DataSent = 0;
        private ContentType _ContentType;
        private DateTime _LastModified;

        private const short BUFFER_SIZE = 1500;

        /// <summary>
        /// Create a new download.
        /// </summary>
        /// <param name="file">The file to send to the client</param>
        public Download(string file)
        {
            _File = file;
            r = new BinaryReader(File.OpenRead(_File));
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
            _ContentType = HttpResponseHeader.GetContentTypeByExtension(info.Extension.Substring(1));
            _LastModified = info.LastWriteTime;
        }

        /// <summary>
        /// Try to send the given file to the client.
        /// </summary>
        /// <param name="handler">The client handler to write to</param>
        /// <param name="response">The response which will be sent to the client</param>
        /// <remarks>
        /// This function will overwrite the content type of the response. Set the content type of the download manually, to the automatical detection.
        /// </remarks>
        public virtual void SendFile(ClientHandler handler, HttpResponse response)
        {
            try
            {
                response.Header.ContentType = _ContentType;
                response.Header.Modified = _LastModified;
                // cache should be used
                if (_Cache != null)
                {
                    if (_Cache.Load(_File))
                    {
                        byte[] toSend = (response.IsContentCompressable(UncompressedLength) && response.UseCompression) ? GzipCompression.Compress(_Cache.Content(_File)) : _Cache.Content(_File);
                        _DataSent = toSend.LongLength;
                        // prepare header
                        response.Header.WriteHeader(handler, (ulong)_DataSent);
                        if (!response.IsHead) handler.SendBytes(toSend);
                        return;
                    }
                }
                // file stream should be used (compressed)
                r = new BinaryReader(File.OpenRead(_File));
                if (response.UseCompression && response.IsContentCompressable(UncompressedLength))
                {
                    byte[] toSend = GzipCompression.Compress(r.ReadBytes((int)r.BaseStream.Length));
                    _DataSent = toSend.LongLength;
                    // prepare header
                    response.Header.WriteHeader(handler, (ulong)_DataSent);
                    if (!response.IsHead) handler.SendBytes(toSend);
                    return;
                }
                // send file stream (uncompressed)
                response.Header.WriteHeader(handler, (ulong)r.BaseStream.Length);
                // send data
                _DataSent = 0;
                short size = BUFFER_SIZE;
                if (_Limit != 0) size = (short)Math.Round(_Limit * 0.0155, 0);
                while (_DataSent < r.BaseStream.Length)
                {
                    int current = size;
                    if ((r.BaseStream.Length - _DataSent) < size) current = (int)(r.BaseStream.Length - _DataSent);
                    if (!response.IsHead) handler.SendBytes(r.ReadBytes(current));
                    _DataSent += current;
                    if (_Limit != 0) Thread.Sleep(1);
                }
            }
            finally
            {
                try
                {
                    // close stream
                    r.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// The length of the file.
        /// </summary>
        /// <remarks>
        /// Will be set after (!) the data has been sent.
        /// </remarks>
        public virtual long Length
        {
            get
            {
                return _DataSent;
            }
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
                    return r.BaseStream.Length;
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
