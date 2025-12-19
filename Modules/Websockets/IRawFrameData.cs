using System.Buffers;

namespace GenHTTP.Modules.Websockets;

public interface IRawFrameData
{

    /// <summary>
    /// Specifies, whether this is a websocket frame that has
    /// been assembled from multiple parts.
    /// </summary>
    bool IsSegmented { get; }

    /// <summary>
    /// True, if this is either a single data frame,
    /// a control frame or the end of a segmented data
    /// stream when in manual continuation mode.
    /// </summary>
    bool Fin { get; }

    /// <summary>
    /// If this is a segmented frame, this property will hold
    /// the non-allocated segments the frame is built from.
    /// </summary>
    List<ReadOnlySequence<byte>?>? Segments { get; }

    /// <summary>
    /// The data the frame consists of, as rented from the underlying
    /// connection.
    /// </summary>
    /// <remarks>
    /// Using this property is only valid while the next frame
    /// has not been read from the connection. Does not allocate.
    /// </remarks>
    ReadOnlySequence<byte> Memory { get; }

}
