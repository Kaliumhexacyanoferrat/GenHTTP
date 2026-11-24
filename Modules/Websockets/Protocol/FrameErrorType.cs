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

    /// <summary>
    /// A reserved bit of the frame header is set, although no extension giving it a meaning was negotiated (RFC 6455, 5.2).
    /// </summary>
    InvalidReservedBits,

    /// <summary>
    /// A continuation frame arrived with no message to continue, or a new message started before the previous one was finished (RFC 6455, 5.4).
    /// </summary>
    InvalidContinuation,

    /// <summary>
    /// A close frame carries a payload of one byte or a status code that may not be sent (RFC 6455, 5.5.1 and 7.4).
    /// </summary>
    InvalidCloseFrame,

    /// <summary>
    /// A text message or the reason of a close frame is not valid UTF-8 (RFC 6455, 8.1).
    /// </summary>
    InvalidUtf8
}
