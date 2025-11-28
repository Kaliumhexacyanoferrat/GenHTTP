namespace GenHTTP.Modules.Straculo.Protocol;

public record WebsocketFrame(ReadOnlyMemory<byte> Data, FrameType Type );