using System.Text;

using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Modules.Reflection.Routing.Segments;

/// <summary>
/// Matches a single segment within a requested path, such as "/segment/".
/// </summary>
/// <param name="segment">The segment to match</param>
internal sealed class StringSegment(string segment) : IRoutingSegment
{
    private readonly ReadOnlyMemory<byte> _segmentBytes = Encoding.ASCII.GetBytes(segment);

    public string[] ProvidedArguments { get; } = [];

    public (bool matched, int offsetBy) TryMatch(IRawRequestTarget target, int offset, ref PathArgumentSink argumentSink)
    {
        var next = target.Next(offset);

        if (next?.Span.SequenceEqual(_segmentBytes.Span) ?? false)
        {
            return (true, 1);
        }

        return (false, 0);
    }

}
