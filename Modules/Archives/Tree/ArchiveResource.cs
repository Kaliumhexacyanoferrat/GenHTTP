using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using SharpCompress.Common;
using SharpCompress.Readers;

namespace GenHTTP.Modules.Archives.Tree;

public class ArchiveResource : IResource
{
    private readonly IResource _archive;

    private readonly string _key;

    public string? Name { get; }

    public DateTime? Modified { get; }

    public FlexibleContentType? ContentType { get; }

    public ulong? Length { get; }

    public ArchiveResource(IResource archive, IEntry entry, string name)
    {
        _archive = archive;

        _key = entry.Key ?? throw new InvalidOperationException("Entry key has to be set");

        Name = name;

        Modified = entry.LastModifiedTime;
        Length = (ulong)entry.Size;

        var guessed = name.GuessContentType();

        if (guessed != null)
        {
            ContentType = new(guessed.Value);
        }
    }

    public ValueTask<ulong> CalculateChecksumAsync() => throw new NotImplementedException();

    public async ValueTask<Stream> GetContentAsync()
    {
        await using var input = await _archive.GetContentAsync();

        using var reader = ReaderFactory.Open(input);

        while (await reader.MoveToNextEntryAsync())
        {
            if (reader.Entry.Key == _key)
            {
                // todo: return a stream that will dispose the underlying stream on disposal
                return await reader.OpenEntryStreamAsync();
            }
        }

        throw new InvalidOperationException($"Unable to find resource '{_key}' in archive");
    }

    public ValueTask WriteAsync(Stream target, uint bufferSize) => throw new NotSupportedException("Writing to archived resources is not supported");

}
