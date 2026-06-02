using fdout;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Files.Handlers;

public class EntryResponseContent : IResponseContent
{
    private readonly RandomAccessCache _cache;

    private readonly Entry _entry;

    public ulong? Length { get; }

    public ContentType? Type { get; }

    public ReadOnlyMemory<byte>? Encoding { get; }

    public EntryResponseContent(RandomAccessCache cache, Entry entry, ContentType? type = null, ReadOnlyMemory<byte>? encoding = null)
    {
        _cache = cache;
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

            var read = _cache.Read(_entry, buffer.Span, offset);

            if (read <= 0)
            {
                break;
            }

            writer.Advance(read);

            offset += read;
        }
        while (offset < _entry.Size);

        return default;
    }

}
