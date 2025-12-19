namespace GenHTTP.Modules.Websockets;

public static class WebsocketFrameSerializationExtensions
{

    /// <summary>
    /// Tries to deserialize the payload of the web socket frame into
    /// an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The expected type of data</typeparam>
    /// <returns>The deserialized data</returns>
    /// <exception cref="InvalidOperationException">Thrown, if no serialization format has been passed to the handler builder or the deserialization attempt fails</exception>
    public static async ValueTask<T> DataAsAsync<T>(this IWebsocketFrame frame)
    {
        var format = frame.Connection.Settings.SerializationFormat ?? throw new InvalidOperationException("The websocket handler has not been initialized with a serializer");

        var buffer = frame.Data.ToArray();

        using var stream = new MemoryStream(buffer);

        stream.Seek(0, SeekOrigin.Begin);

        var result = await format.DeserializeAsync(stream, typeof(T));

        var typed = (T?)result;

        if (typed == null)
        {
            throw new InvalidOperationException("Unable to deserialize the frame into the requested type");
        }

        return typed;
    }

}
