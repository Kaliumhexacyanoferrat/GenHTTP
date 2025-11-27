namespace GenHTTP.Modules.Straculo.Protocol;

public enum FrameType
{
    Continue,
    Text,
    Binary,
    Close,
    Ping,
    Pong
}