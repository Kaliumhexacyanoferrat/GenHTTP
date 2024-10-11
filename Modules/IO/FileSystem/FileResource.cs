using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.FileSystem;

public sealed class FileResource : IResource
{

    #region Initialization

    public FileResource(FileInfo file, string? name, FlexibleContentType? contentType)
    {
        if (!file.Exists)
        {
            throw new FileNotFoundException("File does not exist", file.FullName);
        }

        File = file;

        Name = name ?? file.Name;

        ContentType = contentType ?? FlexibleContentType.Get(Name.GuessContentType() ?? Api.Protocol.ContentType.ApplicationForceDownload);
    }

    #endregion

    #region Get-/Setters

    public FileInfo File { get; }

    public string? Name { get; }

    public DateTime? Modified
    {
        get
        {
            File.Refresh();
            return File.LastWriteTimeUtc;
        }
    }

    public FlexibleContentType? ContentType { get; }

    public ulong? Length
    {
        get
        {
            File.Refresh();
            return (ulong)File.Length;
        }
    }

    #endregion

    #region Functionality

    public ValueTask<Stream> GetContentAsync() => new(File.OpenRead());

    public ValueTask<ulong> CalculateChecksumAsync()
    {
        unchecked
        {
            ulong hash = 17;

            var length = Length;

            hash = hash * 23 + (ulong)Modified.GetHashCode();
            hash = hash * 23 + (length != null ? length.Value : 0);

            return new ValueTask<ulong>(hash);
        }
    }

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        using var content = File.OpenRead();

        await content.CopyPooledAsync(target, bufferSize);
    }

    #endregion

}
