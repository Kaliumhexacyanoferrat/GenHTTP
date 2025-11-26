namespace GenHTTP.Modules.Straculo.Protocol;

public enum FrameType
{
    Continue,
    Utf8,
    Binary,
    Close,
    Ping,
    Pong
}