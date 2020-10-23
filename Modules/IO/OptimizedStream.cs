using System.IO;

using Microsoft.IO;

namespace GenHTTP.Modules.IO
{

    /// <summary>
    /// Uses an array-pooled backend for memory streams which 
    /// reduces GC pressurce.
    /// </summary>
    /// <remarks>
    /// The backend is optimized for our use case which is reading
    /// of (small) request bodies and writing string content 
    /// (typically HTML pages) to responses.
    /// </remarks>
    public static class OptimizedStream
    {

        /// <summary>
        /// Use blocks of 4 KB to handle smaller chunks of memory.
        /// </summary>
        private const int SMALL_BLOCK_SIZE = 4096;

        /// <summary>
        /// Use blocks of 128 KB to handle larger chunks of memory.
        /// </summary>
        private const int LARGE_BLOCK_SIZE = 128 * 1024;

        /// <summary>
        /// Restrict the small memory chunks to 1 MB in total.
        /// </summary>
        private const int SMALL_CACHE_LIMIT = 256 * SMALL_BLOCK_SIZE;

        /// <summary>
        /// Restrict the large memory chunks to 4 MB in total.
        /// </summary>
        private const int LARGE_CACHE_LIMIT = 32 * LARGE_BLOCK_SIZE;

        /// <summary>
        /// Do not use chunks larger than 1 MB per single request.
        /// </summary>
        private const int SINGLE_STREAM_LIMIT = 1024 * 1024;

        private static readonly RecyclableMemoryStreamManager _StreamManager = new RecyclableMemoryStreamManager(SMALL_BLOCK_SIZE, LARGE_BLOCK_SIZE, SINGLE_STREAM_LIMIT)
        {
            MaximumFreeLargePoolBytes = LARGE_CACHE_LIMIT,
            MaximumFreeSmallPoolBytes = SMALL_CACHE_LIMIT
        };

        public static Stream Create() => _StreamManager.GetStream();

        public static Stream From(byte[] buffer) => _StreamManager.GetStream(buffer);

    }

}
