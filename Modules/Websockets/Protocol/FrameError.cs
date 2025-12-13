namespace GenHTTP.Modules.Websockets.Protocol;

public record FrameError(string Message, FrameErrorType ErrorType = FrameErrorType.None)
{
    public const string ReadCanceled = "Read was canceled";
    public const string UnexpectedEndOfStream = "Unexpected end of stream while reading WebSocket frame.";
}