namespace GenHTTP.Modules.Websockets.Protocol;

public record FrameError(string Message, FrameErrorType ErrorType = FrameErrorType.None)
{
    public const string ReadCanceled = "Read was canceled";
    public const string UnexpectedEndOfStream = "Unexpected end of stream while reading WebSocket frame.";

    /// <summary>
    /// The status code to close the connection with because of this error (RFC 6455, 7.4.1),
    /// or null if there is no one left to tell, as when the client went away or the read was canceled.
    /// </summary>
    /// <example>
    /// <code>
    /// if (frame.IsError(out var error) &amp;&amp; error.CloseStatus != null)
    /// {
    ///     await connection.CloseAsync(statusCode: error.CloseStatus.Value);
    /// }
    /// </code>
    /// </example>
    public ushort? CloseStatus => ErrorType switch
    {
        FrameErrorType.InvalidOpCode or FrameErrorType.InvalidControlFrame or FrameErrorType.InvalidControlFrameLength
            or FrameErrorType.InvalidReservedBits or FrameErrorType.InvalidContinuation or FrameErrorType.InvalidCloseFrame => 1002,
        FrameErrorType.InvalidUtf8 => 1007,
        FrameErrorType.PayloadTooLarge => 1009,
        FrameErrorType.UndefinedBehavior => 1011,
        _ => null
    };
}