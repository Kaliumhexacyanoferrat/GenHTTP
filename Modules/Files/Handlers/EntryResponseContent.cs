using fdout;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Files.Handlers;

public class EntryResponseContent : IResponseContent
{
    private readonly Entry _entry;

    public ulong? Length { get; }

    public ContentType? Type { get; }

    public ReadOnlyMemory<byte>? Encoding { get; }

    public EntryResponseContent(Entry entry, ContentType? type = null, ReadOnlyMemory<byte>? encoding = null)
    {
        _entry = entry;

        Length = (ulong)entry.Size;
        Type = type ?? entry.Path.GuessContentType() ?? ContentType.ApplicationOctetStream;
        Encoding = encoding;
    }

    public ValueTask<ulong?> CalculateChecksumAsync()
    {
        unchecked
        {
            ulong hash = 17;

            // todo: modification date
            hash = hash * 23 + (Length ?? 0);

            return new(hash);
        }
    }

    public ValueTask WriteAsync(IResponseSink sink)
    {
        var writer = sink.Writer;

        long offset = 0;

        do
        {
            var buffer = writer.GetMemory(64 * 1024);

            var want = (int)Math.Min(buffer.Length, _entry.Size - offset);
        };
    }

}
