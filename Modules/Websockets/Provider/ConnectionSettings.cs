using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;

namespace GenHTTP.Modules.Websockets.Provider;

/// <summary>
/// Provides the settings used by a websocket connection.
/// </summary>
/// <param name="Formatters">The formatter registry to be used to send and receive formatted data</param>
/// <param name="SerializationFormat">The serialization handler to send and receive structured data</param>
/// <param name="RxBufferSize">The size a websocket frame received from the server can be at maximum. Defaults to 16 KB.</param>
/// <param name="HandleContinuationFramesManually">Whether the API user is expected to handle continuation frames manually</param>
/// <param name="AllocateFrameData">Whether data will automatically be allocated so it can still be accessed after the next message has been read</param>
public record ConnectionSettings(

    FormatterRegistry Formatters,

    ISerializationFormat SerializationFormat,

    int RxBufferSize,

    bool HandleContinuationFramesManually,

    bool AllocateFrameData

);
