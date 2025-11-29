namespace GenHTTP.Modules.Straculo.Protocol;

public enum FrameType
{
    None,
    Text,
    Binary,
    Continue,
    Close,
    Ping,
    Pong,
    Error,
}