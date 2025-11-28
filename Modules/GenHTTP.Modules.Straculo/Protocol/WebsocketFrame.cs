namespace GenHTTP.Modules.Straculo.Protocol;

public record WebsocketFrame(
    ReadOnlyMemory<byte> Data, 
    FrameType Type = FrameType.Close, 
    FrameError? FrameError = null);