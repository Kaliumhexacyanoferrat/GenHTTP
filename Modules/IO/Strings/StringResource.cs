using System.Text;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Strings;

public sealed class StringResource : IResource
{
    private static readonly Encoding Utf8 = Encoding.UTF8;

    #region Initialization

    public StringResource(string content, string? name, FlexibleContentType? contentType, DateTime? modified)
    {
        Content = Utf8.GetBytes(content);
        Checksum = (ulong)content.GetHashCode();

        Name = name;
        ContentType = contentType ?? FlexibleContentType.Get(Api.Protocol.ContentType.TextPlain);
        Modified = modified;
    }

    #endregion

    #region Get-/Setters

    public byte[] Content { get; }

    public string? Name { get; }

    public DateTime? Modified { get; }

    public FlexibleContentType? ContentType { get; }

    public ulong? Length => (ulong)Content.Length;

    private ulong Checksum { get; }

    #endregion

    #region Functionality

    public ValueTask<Stream> GetContentAsync() => new(new MemoryStream(Content, false));

    public ValueTask<ulong> CalculateChecksumAsync() => new(Checksum);

    public ValueTask WriteAsync(Stream target, uint bufferSize) => target.WriteAsync(Content.AsMemory());

    #endregion

}
