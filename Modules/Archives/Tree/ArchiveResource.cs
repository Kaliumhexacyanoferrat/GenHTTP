using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using SharpCompress.Common;

namespace GenHTTP.Modules.Archives.Tree;

internal sealed class ArchiveResource : IResource
{
    private readonly IResource _archive;

    private readonly Func<Stream, string, ValueTask<ArchiveHandle>> _handleFactory;

    private readonly string _key;

    #region Get-/Setters

    public string? Name { get; }

    public DateTime? Modified { get; }

    public FlexibleContentType? ContentType { get; }

    public ulong? Length { get; }

    #endregion

    #region Initialization

    internal ArchiveResource(IResource archive, IEntry entry, string name, Func<Stream, string, ValueTask<ArchiveHandle>> handleFactory)
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

    #endregion

    #region Functionality

    public ValueTask<ulong> CalculateChecksumAsync() => new(Checksum.Calculate(this));

    public async ValueTask<Stream> GetContentAsync()
    {
        var input = await _archive.GetContentAsync();

        var handle = await _handleFactory(input, _key);

        return new ArchiveEntryStream(handle);
    }

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        var content = await GetContentAsync();

        await content.CopyToAsync(target, (int)bufferSize);
    }

    #endregion

}
