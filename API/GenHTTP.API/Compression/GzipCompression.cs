using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace GenHTTP.Api.Compression
{

    /// <summary>
    /// Provides GZip compression.
    /// </summary>
    public class GzipCompression
    {

        /// <summary>
        /// Compress data.
        /// </summary>
        /// <param name="toCompress">The data to compress</param>
        /// <returns>The compressed data</returns>
        public static byte[] Compress(byte[] toCompress)
        {
            MemoryStream write = new MemoryStream();
            GZipStream compressed = new GZipStream(write, CompressionMode.Compress);
            compressed.Write(toCompress, 0, toCompress.Length);
            compressed.Close();
            return write.ToArray();
        }

    }

}
