using System.Text;

namespace GenHTTP.Modules.Straculo.Protocol;

public record WebsocketFrame(
    ReadOnlyMemory<byte> Data,
    FrameType Type = FrameType.Close,
    bool Fin = false,
    FrameError? FrameError = null)
{
    public string DataAsString => Encoding.UTF8.GetString(Data.Span);
}


// TODO: Is it work it to use a immutable struct here?
public readonly struct WebsocketFrameStruct
{
    public ReadOnlyMemory<byte> Payload { get; }
    public FrameType Type { get; }
    public bool Fin { get; }
    public FrameError? FrameError { get; }

}