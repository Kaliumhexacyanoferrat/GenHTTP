using System.Buffers;
using System.Text;

namespace GenHTTP.Modules.Websockets.Protocol;

public sealed class WebsocketFrame
{
    public bool IsSegmentedFrame { get; internal set; }
    
    public List<ReadOnlySequence<byte>?>? SegmentedRawData { get; internal set; } // Can be useful to examine segments separately

    public ReadOnlySequence<byte> RawData { get; internal set; } // Accessing does not allocate

    private ReadOnlyMemory<byte>? _cachedData;
    //public ReadOnlyMemory<byte> Data => RawData?.ToArray() ?? ReadOnlyMemory<byte>.Empty; // Allocates
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
    
    public string DataAsString() => Encoding.UTF8.GetString(Data.ToArray()); // Can allocate twice
    
    // For very rare use cases when the frame scope is outside the pipe reader internal buffer valid span
    // e.g. Ping/Pong frames amid segmented frames
    public void SetCachedData() => _cachedData = RawData.ToArray();
    
    
    public FrameType Type { get; }
    
    public bool Fin { get; }
    
    public FrameError? FrameError { get; private set; }
    
    
    public WebsocketFrame(
        ref ReadOnlySequence<byte> rawData, 
        FrameType type,
        bool fin = true)
    {
        RawData = rawData;
        Type = type;
        Fin = fin;
    }
    
    public WebsocketFrame(
        FrameType type, 
        bool fin = true)
    {
        Type = type;
        Fin = fin;
    }

    // Constructor overload for error frames
    public WebsocketFrame(
        FrameError frameError, // Error is required here
        FrameType type = FrameType.Error, 
        bool fin = true)
    {
        Type = type;
        Fin = fin;
        FrameError = frameError;
    }

    // For pooled object case
    public void Clear()
    {
        IsSegmentedFrame = false;
        SegmentedRawData?.Clear();
        RawData = default;
        _cachedData = null;
        
        FrameError = null;
    }
}