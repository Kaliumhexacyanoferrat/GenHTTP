using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Strings;

public sealed class StringResource : IResource
{
    private static readonly Encoding UTF8 = Encoding.UTF8;

    #region Get-/Setters

    public byte[] Content { get; }

    public string? Name { get; }

    public DateTime? Modified { get; }

    public FlexibleContentType? ContentType { get; }

    public ulong? Length => (ulong)Content.Length;

    private ulong Checksum { get; }

    #endregion

    #region Initialization

    public StringResource(string content, string? name, FlexibleContentType? contentType, DateTime? modified)
    {
            Content = UTF8.GetBytes(content);
            Checksum = (ulong)content.GetHashCode();

            Name = name;
            ContentType = contentType ?? FlexibleContentType.Get(Api.Protocol.ContentType.TextPlain);
            Modified = modified;
        }

    #endregion

    #region Functionality

    public ValueTask<Stream> GetContentAsync() => new(new MemoryStream(Content, false));

    public ValueTask<ulong> CalculateChecksumAsync()
    {
            return new ValueTask<ulong>(Checksum);
        }

    public ValueTask WriteAsync(Stream target, uint bufferSize) => target.WriteAsync(Content.AsMemory());

    #endregion

}
