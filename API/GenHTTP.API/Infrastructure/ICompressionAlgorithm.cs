using System.IO;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// The implementation of an algorithm allowing to transfer content
    /// in a compressed form to the client.
    /// </summary>
    public interface ICompressionAlgorithm
    {

        /// <summary>
        /// The name of the algorithm as specified by the client in the
        /// "Accept-Encoding" HTTP header.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The priority of the algorithm. The algorithm with the highest
        /// priority will be selected if mutliple algorithms can be applied
        /// to a response.
        /// </summary>
        Priority Priority { get; }

        /// <summary>
        /// Returns a content instance allowing the server to stream the compressed content.
        /// </summary>
        /// <param name="content">The content of the response to be compressed</param>
        /// <returns>A result representing the compressed content</returns>
        IResponseContent Compress(IResponseContent content);

    }

}
