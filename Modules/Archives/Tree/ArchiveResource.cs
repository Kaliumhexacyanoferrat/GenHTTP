using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;
using SharpCompress.Common;

namespace GenHTTP.Modules.Archives.Tree;

internal sealed class ArchiveResource : IResource
{
    private readonly IResource _archive;

    private readonly Func<Stream, string, StreamWithDependency> _streamFactory;

    private readonly string _key;

    #region Get-/Setters

    public string? Name { get; }

    public DateTime? Modified { get; }

    public FlexibleContentType? ContentType { get; }

    public ulong? Length { get; }

    #endregion

    #region Initialization

    internal ArchiveResource(IResource archive, IEntry entry, string name, Func<Stream, string, StreamWithDependency> streamFactory)
    {
        _archive = archive;
        _streamFactory = streamFactory;

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

        return _streamFactory(input, _key);
    }

    #endregion

}
