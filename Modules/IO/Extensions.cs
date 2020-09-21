using System.IO;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.IO
{

    public static class Extensions
    {


        /// <summary>
        /// Sends the given stream to the client.
        /// </summary>
        /// <param name="stream">The stream to be sent</param>
        public static IResponseBuilder Content(this IResponseBuilder builder, Stream stream) => builder.Content(new StreamContent(stream));

        /// <summary>
        /// Sends the given string to the client.
        /// </summary>
        /// <param name="text">The string to be sent</param>
        public static IResponseBuilder Content(this IResponseBuilder builder, string text) => builder.Content(new StringContent(text));

    }

}
