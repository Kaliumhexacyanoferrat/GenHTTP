using System;
using System.IO;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO
{

    public static class ResponseBuilderExtensions
    {

        /// <summary>
        /// Sends the given resource to the client.
        /// </summary>
        /// <param name="resource">The resource to be sent</param>
        /// <remarks>
        /// This method will set the content, but not the content
        /// type of the response.
        /// </remarks>
        public static IResponseBuilder Content(this IResponseBuilder builder, IResource resource) => builder.Content(resource.GetContent(), () => resource.GetChecksum());

        /// <summary>
        /// Sends the given stream to the client.
        /// </summary>
        /// <param name="stream">The stream to be sent</param>
        /// <param name="checksumProvider">The logic to efficiently calculate checksums</param>
        public static IResponseBuilder Content(this IResponseBuilder builder, Stream stream, Func<ulong?> checksumProvider) => builder.Content(new StreamContent(stream, checksumProvider));

        /// <summary>
        /// Sends the given string to the client.
        /// </summary>
        /// <param name="text">The string to be sent</param>
        public static IResponseBuilder Content(this IResponseBuilder builder, string text) => builder.Content(Resource.FromString(text).Type(ContentType.TextPlain).Build());

        /// <summary>
        /// Efficiently calculates the checksum of the stream, beginning
        /// from the current position. Resets the position to the previous
        /// one.
        /// </summary>
        /// <returns>The checksum of the stream</returns>
        public static ulong? CalculateChecksum(this Stream stream)
        {
            if (stream.CanSeek)
            {
                var position = stream.Position;

                try
                {
                    unchecked
                    {
                        ulong hash = 17;

                        Span<byte> buffer = stackalloc byte[128];

                        var read = 0;

                        do
                        {
                            read = stream.Read(buffer);

                            for (int i = 0; i < read; i++)
                            {
                                hash = hash * 23 + buffer[i];
                            }
                        }
                        while (read > 0);

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

    }

}
