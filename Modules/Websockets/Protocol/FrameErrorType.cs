namespace GenHTTP.Modules.Websockets.Protocol;

public enum FrameErrorType
{
    None,
    Incomplete,
    InvalidOpCode,
    PayloadTooLarge,
    InvalidControlFrame,
    InvalidControlFrameLength,
    Canceled,
    IncompleteForever, // PipeReader completed without a full frame
    UndefinedBehavior,
    InvalidContinuationFrame
}