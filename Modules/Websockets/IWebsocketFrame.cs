using System.Diagnostics.CodeAnalysis;

using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets;

public interface IWebsocketFrame
{

    /// <summary>
    /// The type of this frame.
    /// </summary>
    FrameType Type { get; }

    /// <summary>
    /// The data the frame consists of.
    /// </summary>
    /// <remarks>
    /// Has to be read while the next frame has not been read from
    /// the connection. Afterward this property is safe to be used,
    /// independent of the connection lifecycle.
    /// </remarks>
    ReadOnlyMemory<byte> Data { get; }

    /// <summary>
    /// Provides additional, technical information about the frame as well
    /// as access to the raw memory of the underlying buffer.
    /// </summary>
    /// <remarks>
    /// While accessing <see cref="Data"/> allocates the data into a buffer
    /// so it can be accessed after the next frame has been read, the
    /// raw view allows to directly access the memory as it has been
    /// read from the underlying connection.
    /// </remarks>
    IRawFrameData Raw { get; }

    /// <summary>
    /// Checks, whether the frame represents an error.
    /// </summary>
    /// <param name="error">If this is an error frame, the actual error that happened</param>
    /// <returns>true, if this is an error frame, false otherwise</returns>
    bool IsError([MaybeNullWhen(returnValue: false)] out FrameError error);

}
