namespace GenHTTP.Modules.Websockets.Protocol;

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