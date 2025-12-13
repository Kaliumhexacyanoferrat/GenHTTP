using System.Buffers;
using System.Text;

namespace GenHTTP.Modules.Websockets.Protocol;

public sealed class WebsocketFrame
{
    public ReadOnlySequence<byte>? RawData { get; }
    public FrameType Type { get; }
    public bool Fin { get; }
    public FrameError? FrameError { get; }
    
    public ReadOnlyMemory<byte> Data => RawData?.ToArray() ?? ReadOnlyMemory<byte>.Empty;

    public string DataAsString() => Encoding.UTF8.GetString(Data.ToArray());
    
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

public sealed class BufferSegment : ReadOnlySequenceSegment<byte>
{
    public BufferSegment(ReadOnlyMemory<byte> memory) => Memory = memory;

    public BufferSegment Append(ReadOnlyMemory<byte> memory)
    {
        var next = new BufferSegment(memory)
        {
            RunningIndex = RunningIndex + Memory.Length
        };
        Next = next;
        return next;
    }
}

public static class SequenceUtils
{
    public static ReadOnlySequence<byte> ConcatSequences(params ReadOnlySequence<byte>[] sequences)
    {
        BufferSegment? first = null;
        BufferSegment? last = null;

        foreach (var seq in sequences)
        {
            foreach (var mem in seq) // mem is ReadOnlyMemory<byte> for each segment
            {
                if (mem.Length == 0) continue;

                if (first is null)
                    first = last = new BufferSegment(mem);
                else
                    last = last!.Append(mem);
            }
        }

        if (first is null)
            return ReadOnlySequence<byte>.Empty;

        return new ReadOnlySequence<byte>(first, 0, last!, last!.Memory.Length);
    }
}