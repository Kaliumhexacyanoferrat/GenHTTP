using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Engine.Shared.Types;

using Glyph11.Parser.Hardened;

namespace GenHTTP.Engine.Internal.Parser;

public static class RequestParser
{

    public static async Task<SequencePosition?> TryParseAsync(
        PipeReader reader,
        Request into,
        CancellationToken ct = default)
    {
        while (true)
        {
            var result = await reader.ReadAsync(ct);
            var buffer = result.Buffer;

            if (TryParse(buffer, into, out var consumed))
            {
                into.Apply();
                return buffer.GetPosition(consumed);
            }

            if (result.IsCompleted)
                return null;
        }
    }

    public static bool TryParse(ReadOnlySequence<byte> buffer, Request into, out int bytesRead)
        => HardenedParser.TryExtractFullHeader(ref buffer, into.Source, ParserLimits.Default, out bytesRead);

}
