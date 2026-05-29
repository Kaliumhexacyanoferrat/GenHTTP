using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.FileSystem;

public sealed class FileResource : IResource
{
    private long _length;

    private readonly string _fullPath;

    #region Initialization

    public FileResource(FileInfo file, string? name, ContentType? contentType)
    {
        if (!file.Exists)
        {
            throw new FileNotFoundException("File does not exist", file.FullName);
        }

        File = file;

        _fullPath = file.FullName;
        _length = file.Length;

        Name = name ?? file.Name;

        ContentType = contentType ?? Name.GuessContentType() ?? Api.Protocol.ContentType.ApplicationForceDownload;
    }

    #endregion

    #region Get-/Setters

    public FileInfo File { get; }

    public string? Name { get; }

    public DateTime? Modified => File.LastWriteTimeUtc;

    public ContentType? ContentType { get; }

    public ulong? Length => (ulong)_length;

    #endregion

    #region Functionality

    public ValueTask<Stream> GetContentAsync() => new(File.OpenRead());

    public ValueTask<ulong> CalculateChecksumAsync()
    {
        File.Refresh();

        _length = File.Length;
        
        return new(Checksum.Calculate(this));
    }

    public async ValueTask WriteAsync(IResponseSink sink)
    {
        await using var stream = new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.SequentialScan);

        _length = stream.Length;
        
        await stream.WriteAsync(sink.Writer);
    } 

    #endregion

}
