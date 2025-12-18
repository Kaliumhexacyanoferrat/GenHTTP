using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websockets.Protocol;

/// <summary>
/// A single frame read from the websocket connection.
/// </summary>
public sealed class WebsocketFrame : IWebsocketFrame, IRawFrameData
{
    private ReadOnlyMemory<byte>? _cachedData;

    private readonly FrameError? _frameError = null;

    #region Get-/Setters

    public IRawFrameData Raw => this;

    public bool IsSegmented { get; internal set; }

    public List<ReadOnlySequence<byte>?>? Segments { get; internal set; }

    public ReadOnlySequence<byte> Memory { get; internal set; }

    public ReadOnlyMemory<byte> Data
    {
        get
        {
            if (_cachedData.HasValue)
                return _cachedData.Value;

            _cachedData = Memory.ToArray();
            return _cachedData.Value;
        }
    }

    public FrameType Type { get; }

    public bool Fin { get; }

    #endregion

    #region Initialization

    internal WebsocketFrame(ref ReadOnlySequence<byte> rawData, FrameType type, bool fin = true)
    {
        Memory = rawData;
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
    internal void SetCachedData() => _cachedData = Memory.ToArray();

    #endregion

}
