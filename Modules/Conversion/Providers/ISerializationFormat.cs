using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Providers
{

    /// <summary>
    /// Implements a specific format that can be used to serialize
    /// and deserialize data within service oriented handler.
    /// </summary>
    /// <remarks>
    /// Instances implementing this interface can be registered at a 
    /// <see cref="SerializationRegistry" /> to support additional 
    /// transport formats.
    /// </remarks>
    public interface ISerializationFormat
    {

        /// <summary>
        /// Deserializes the given stream into an object of the given type.
        /// </summary>
        /// <param name="stream">The stream providing the data to be deserialized</param>
        /// <param name="type">The type to be deserialized</param>
        /// <returns>The object deserialized from the given stream</returns>
        Task<object> Deserialize(Stream stream, Type type);

        /// <summary>
        /// Serializes the given response into a new response for the
        /// given request.
        /// </summary>
        /// <param name="request">The request to generate a response for</param>
        /// <param name="response">The object to be serialized</param>
        /// <returns>The response representing the serialized object</returns>
        IResponseBuilder Serialize(IRequest request, object response);

    }

}
