using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websockets.Protocol;

/// <summary>
/// A single frame read from the websocket connection.
/// </summary>
public sealed class WebsocketFrame
{
    private ReadOnlyMemory<byte>? _cachedData;

    private readonly FrameError? _frameError = null;

    #region Get-/Setters

    /// <summary>
    /// Specifies, whether this is a websocket frame that has
    /// been assembled from multiple parts.
    /// </summary>
    public bool IsSegmentedFrame { get; internal set; }

    /// <summary>
    /// If this is a segmented frame, this property will hold
    /// the segments the frame is built from.
    /// </summary>
    public List<ReadOnlySequence<byte>?>? SegmentedRawData { get; internal set; }

    /// <summary>
    /// The data the frame consists of.
    /// </summary>
    /// <remarks>
    /// Using this property is only valid while the next frame
    /// has not been read from the connection. Does not allocate.
    /// </remarks>
    public ReadOnlySequence<byte> RawData { get; internal set; }

    /// <summary>
    /// The data the frame consists of.
    /// </summary>
    /// <remarks>
    /// Has to be read while the next frame has not been read from
    /// the connection. Afterward this property is safe to be used,
    /// independent of the connection lifecycle.
    /// </remarks>
    public ReadOnlyMemory<byte> Data
    {
        get
        {
            if (_cachedData.HasValue)
                return _cachedData.Value;

            _cachedData = RawData.ToArray();
            return _cachedData.Value;
        }
    }

    /// <summary>
    /// The type of this frame.
    /// </summary>
    public FrameType Type { get; }

    /// <summary>
    /// True, if this is either a single data frame,
    /// a control frame or the end of a segmented data
    /// stream when in manual continuation mode.
    /// </summary>
    public bool Fin { get; }

    #endregion

    #region Initialization

    internal WebsocketFrame(ref ReadOnlySequence<byte> rawData, FrameType type, bool fin = true)
    {
        RawData = rawData;
        Type = type;
        Fin = fin;
    }

    internal WebsocketFrame(FrameType type, bool fin = true)
    {
        Type = type;
        Fin = fin;
    }

    // Constructor overload for error frames
    internal WebsocketFrame(FrameError frameError, FrameType type = FrameType.Error, bool fin = true)
    {
        Type = type;
        Fin = fin;

        _frameError = frameError;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Checks, whether the frame represents an error.
    /// </summary>
    /// <param name="error">If this is an error frame, the actual error that happened</param>
    /// <returns>true, if this is an error frame, false otherwise</returns>
    public bool IsError([MaybeNullWhen(returnValue: false)] out FrameError error)
    {
        if ((Type == FrameType.Error) && (_frameError != null))
        {
            error = _frameError;
            return true;
        }

        error = null;
        return false;
    }

    /// <summary>
    /// Reads the data provided by the websocket frame
    /// as a UTF-8 string.
    /// </summary>
    /// <returns>The UTF-8 encoded string data of the frame</returns>
    public string DataAsString() => Encoding.UTF8.GetString(Data.ToArray()); // Can allocate twice

    // For very rare use cases when the frame scope is outside the pipe reader internal buffer valid span
    // e.g. Ping/Pong frames amid segmented frames
    internal void SetCachedData() => _cachedData = RawData.ToArray();

    #endregion

}
