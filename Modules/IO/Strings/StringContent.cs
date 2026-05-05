using System.Buffers;
using System.Text;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Strings;

public sealed class StringContent : IResponseContent
{
    private static readonly Encoding Utf8 = Encoding.UTF8;

    private static readonly ReadOnlyMemory<byte> DefaultContentType = "text/plain"u8.ToArray();

    private readonly ulong _checksum;

    private readonly byte[] _content;
    
    private readonly ReadOnlyMemory<byte> _contentType;

    #region Initialization

    public StringContent(string content, ReadOnlyMemory<byte>? contentType = null)
    {
        _content = Utf8.GetBytes(content);

        Length = (ulong)_content.Length;

        _checksum = (ulong)content.GetHashCode();

        _contentType = contentType ?? DefaultContentType;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length { get; }

    public ReadOnlyMemory<byte> Type => _contentType;

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
