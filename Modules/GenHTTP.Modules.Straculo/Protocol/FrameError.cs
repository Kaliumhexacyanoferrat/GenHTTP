namespace GenHTTP.Modules.Straculo.Protocol;

public record FrameError(string Message, FrameErrorType ErrorType = FrameErrorType.None);