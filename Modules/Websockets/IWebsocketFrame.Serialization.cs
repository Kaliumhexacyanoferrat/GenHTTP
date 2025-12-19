using System.Text;

namespace GenHTTP.Modules.Websockets;

public static class WebsocketFrameSerializationExtensions
{

    /// <summary>
    /// Tries to read the payload of the web socket frame into an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The expected type of data</typeparam>
    /// <returns>The deserialized data</returns>
    /// <exception cref="InvalidOperationException">Thrown, if no serialization format has been passed to the handler builder or the deserialization attempt fails</exception>
    public static async ValueTask<T> ReadPayloadAsync<T>(this IWebsocketFrame frame)
    {
        if (frame.Connection.Settings.Formatters.CanHandle(typeof(T)))
        {
            var stringData = Encoding.UTF8.GetString(frame.Data.ToArray());

            return Enforce<T>(frame.Connection.Settings.Formatters.Read(stringData, typeof(T)));
        }

        var buffer = frame.Data.ToArray();

        using var stream = new MemoryStream(buffer);

        stream.Seek(0, SeekOrigin.Begin);

        var result = await frame.Connection.Settings.SerializationFormat.DeserializeAsync(stream, typeof(T));

        return Enforce<T>(result);
    }

    private static T Enforce<T>(object? result) => (T)result!;

}
