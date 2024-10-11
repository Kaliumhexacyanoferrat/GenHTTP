using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Protobuf;

namespace GenHTTP.Testing;

public static class ContentExtensions
{

    /// <summary>
    /// Reads the response body as a string.
    /// </summary>
    /// <param name="response">The response to read</param>
    /// <returns>The content of the HTTP response</returns>
    public static async ValueTask<string> GetContentAsync(this HttpResponseMessage response) => await response.Content.ReadAsStringAsync();

    /// <summary>
    /// Deserializes the payload of the HTTP response into the given type.
    /// </summary>
    /// <typeparam name="T">The type of the payload to be read</typeparam>
    /// <param name="response">The response to read the payload from</param>
    /// <returns>The deserialized payload of the response</returns>
    /// <exception cref="InvalidOperationException">Thrown if the format returned by the server is not supported</exception>
    /// <remarks>
    /// This method supports all formats that ship with the GenHTTP framework (JSON, XML, form encoded, Protobuf)
    /// and falls back to JSON if the server does not indicate a content type.
    /// </remarks>
    public static async ValueTask<T> GetContentAsync<T>(this HttpResponseMessage response)
    {
            return await response.GetOptionalContentAsync<T>() ?? throw new InvalidOperationException("Server did not return a result");
        }

    /// <summary>
    /// Attempts to deserialize the payload of the HTTP response into the given type.
    /// </summary>
    /// <typeparam name="T">The type of the payload to be read</typeparam>
    /// <param name="response">The response to read the payload from</param>
    /// <returns>The deserialized payload of the response or null, if the server did not return data</returns>
    /// <exception cref="InvalidOperationException">Thrown if the format returned by the server is not supported</exception>
    /// <remarks>
    /// This method supports all formats that ship with the GenHTTP framework (JSON, XML, form encoded, Protobuf)
    /// and falls back to JSON if the server does not indicate a content type.
    /// </remarks>
    public static async ValueTask<T?> GetOptionalContentAsync<T>(this HttpResponseMessage response)
    {
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default;
            }

            var type = response.GetContentHeader("Content-Type");

            var registry = Serialization.Default()
                                        .AddProtobuf()
                                        .Build();

            var format = registry.GetFormat(type) ?? throw new InvalidOperationException($"Unable to find deserializer for content type '{type}'");

            using var body = await response.Content.ReadAsStreamAsync();

            return (T?)await format.DeserializeAsync(body, typeof(T));
        }

}
