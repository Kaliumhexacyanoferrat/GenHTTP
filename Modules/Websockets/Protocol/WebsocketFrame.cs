using System.Buffers;
using System.Text;

namespace GenHTTP.Modules.Websockets.Protocol;

public sealed class WebsocketFrame
{
    public bool IsSegmentedFrame { get; internal set; }
    
    public List<ReadOnlySequence<byte>?>? SegmentedRawData { get; internal set; } // Can be useful to examine segments separately

    public ReadOnlySequence<byte>? RawData { get; internal set; } // Accessing does not allocate
    
    public ReadOnlyMemory<byte> Data => RawData?.ToArray() ?? ReadOnlyMemory<byte>.Empty; // Allocates
    
    public string DataAsString() => Encoding.UTF8.GetString(Data.ToArray()); // Can allocate twice
    
    
    public FrameType Type { get; internal set; }
    
    public bool Fin { get; }
    
    public FrameError? FrameError { get; }
    
    
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
}