using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using SharpCompress.Common;

namespace GenHTTP.Modules.Archives.Tree;

public class ArchiveResource : IResource
{
    private readonly IResource _archive;

    private readonly Func<Stream, string, ValueTask<ArchiveHandle>> _handleFactory;

    private readonly string _key;

    public string? Name { get; }

    public DateTime? Modified { get; }

    public FlexibleContentType? ContentType { get; }

    public ulong? Length { get; }

    public ArchiveResource(IResource archive, IEntry entry, string name, Func<Stream, string, ValueTask<ArchiveHandle>> handleFactory)
    {
        _archive = archive;
        _handleFactory = handleFactory;

        _key = entry.Key ?? throw new InvalidOperationException("Entry key has to be set");

        Name = name;

        Modified = entry.LastModifiedTime;

        if (entry.Size > 0)
        {
            Length = (ulong)entry.Size;
        }

        var guessed = name.GuessContentType();

        if (guessed != null)
        {
            ContentType = new(guessed.Value);
        }
    }

    public ValueTask<ulong> CalculateChecksumAsync() => throw new NotImplementedException();

    public async ValueTask<Stream> GetContentAsync()
    {
        var input = await _archive.GetContentAsync();

        var handle = await _handleFactory(input, _key);

        return new ArchiveEntryStream(handle);
    }

    public ValueTask WriteAsync(Stream target, uint bufferSize) => throw new NotSupportedException("Writing to archived resources is not supported");

}
