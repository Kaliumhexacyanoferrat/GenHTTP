using System.Buffers;
using System.Text;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Strings;

public sealed class StringContent : IResponseContent
{
    private static readonly Encoding Utf8 = System.Text.Encoding.UTF8;

    private readonly ulong _checksum;

    private readonly byte[] _content;

    #region Initialization

    public StringContent(string content, ContentType? contentType = null)
    {
        _content = Utf8.GetBytes(content);

        Length = (ulong)_content.Length;

        _checksum = (ulong)content.GetHashCode();

        Type = contentType ?? ContentType.TextPlain;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length { get; }

    public ContentType? Type { get; }

    public ReadOnlyMemory<byte>? Encoding => null;

    #endregion

    #region Functionality

    /*public ValueTask<ulong?> CalculateChecksumAsync() => new(_checksum);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.WriteAsync(_content.AsMemory());
    }*/

    public ValueTask WriteAsync(IResponseSink sink)
    {
        sink.Writer.Write(_content.AsSpan());
        return ValueTask.CompletedTask;
    }

    #endregion

}
