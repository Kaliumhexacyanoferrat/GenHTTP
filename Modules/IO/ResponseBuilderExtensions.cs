using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO
{

    public static class ResponseBuilderExtensions
    {
        private static readonly ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        /// <summary>
        /// Sends the given string to the client.
        /// </summary>
        /// <param name="text">The string to be sent</param>
        public static IResponseBuilder Content(this IResponseBuilder builder, string text) => builder.Content(Resource.FromString(text).Type(ContentType.TextPlain).Build());

        /// <summary>
        /// Sends the given resource to the client.
        /// </summary>
        /// <param name="resource">The resource to be sent</param>
        /// <remarks>
        /// This method will set the content, but not the content
        /// type of the response.
        /// </remarks>
        public static IResponseBuilder Content(this IResponseBuilder builder, IResource resource) => builder.Content(new ResourceContent(resource));

        /// <summary>
        /// Sends the given stream to the client.
        /// </summary>
        /// <param name="stream">The stream to be sent</param>
        /// <param name="knownLength">The known length of the stream (if the stream does not propagate this information)</param>
        /// <param name="checksumProvider">The logic to efficiently calculate checksums</param>
        public static IResponseBuilder Content(this IResponseBuilder builder, Stream stream, ulong? knownLength, Func<ValueTask<ulong?>> checksumProvider) => builder.Content(new StreamContent(stream, knownLength, checksumProvider));

        /// <summary>
        /// Sends the given stream to the client.
        /// </summary>
        /// <param name="stream">The stream to be sent</param>
        /// <param name="checksumProvider">The logic to efficiently calculate checksums</param>
        public static IResponseBuilder Content(this IResponseBuilder builder, Stream stream, Func<ValueTask<ulong?>> checksumProvider) => builder.Content(stream, null, checksumProvider);

        /// <summary>
        /// Efficiently calculates the checksum of the stream, beginning
        /// from the current position. Resets the position to the previous
        /// one.
        /// </summary>
        /// <returns>The checksum of the stream</returns>
        public static async ValueTask<ulong?> CalculateChecksumAsync(this Stream stream)
        {
            if (stream.CanSeek)
            {
                var position = stream.Position;

                try
                {
                    unchecked
                    {
                        ulong hash = 17;

                        var buffer = POOL.Rent(4096);

                        try
                        {
                            var read = 0;

                            do
                            {
                                read = await stream.ReadAsync(buffer).ConfigureAwait(false);

                                for (int i = 0; i < read; i++)
                                {
                                    hash = hash * 23 + buffer[i];
                                }
                            }
                            while (read > 0);
                        }
                        finally
                        {
                            POOL.Return(buffer);
                        }

                        return hash;
                    }
                }
                finally
                {
                    stream.Seek(position, SeekOrigin.Begin);
                }
            }

            return null;
        }

        public static ValueTask<IResponse?> BuildTask(this IResponseBuilder builder) => new ValueTask<IResponse?>(builder.Build());

    }

}
