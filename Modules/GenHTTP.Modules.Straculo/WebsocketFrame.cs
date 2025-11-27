using GenHTTP.Modules.Straculo.Protocol;

namespace GenHTTP.Modules.Straculo;

public record WebsocketFrame(ReadOnlyMemory<byte> Data, FrameType Type );