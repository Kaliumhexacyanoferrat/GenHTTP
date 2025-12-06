namespace GenHTTP.Modules.Websockets.Protocol;

public record FrameError(string Message, FrameErrorType ErrorType = FrameErrorType.None);