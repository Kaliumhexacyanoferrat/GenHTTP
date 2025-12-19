using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets;

public static class ConnectionSerializationExtensions
{

    /// <summary>
    /// Serializes the given payload using the serialization format passed
    /// to the web socket builder and sends it as a websocket frame.
    /// </summary>
    /// <param name="payload">The data to be sent</param>
    /// <param name="opcode">The type of data to be sent (either text or binary)</param>
    /// <param name="token">The cancellation token to use for this operation</param>
    /// <typeparam name="T">The type of data to be sent</typeparam>
    /// <exception cref="InvalidOperationException">Thrown, if no serialization format was passed to the websocket handler builder</exception>
    /// <exception cref="ArgumentNullException">Thrown, if null has been passed as a payload</exception>
    public static ValueTask WriteAsync<T>(this ISocketConnection connection, T payload, FrameType opcode = FrameType.Text, CancellationToken token = default)
    {
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        var format = connection.Settings.SerializationFormat ?? throw new InvalidOperationException("The websocket handler has not been initialized with a serializer");

        return connection.WriteAsync(format, opcode, payload, token);
    }

    private static async ValueTask WriteAsync(this ISocketConnection connection, ISerializationFormat format, FrameType opcode, object payload, CancellationToken token)
    {
        var buffer = await format.SerializeAsync(payload);

        await connection.WriteAsync(buffer, opcode, true, token);
    }

}
