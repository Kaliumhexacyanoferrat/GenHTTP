using System.Text;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Strings;

public sealed class StringContent : IResponseContent
{
    private static readonly Encoding Utf8 = Encoding.UTF8;

    private readonly ulong _checksum;

    private readonly byte[] _content;

    #region Initialization

    public StringContent(string content)
    {
        _content = Utf8.GetBytes(content);

        Length = (ulong)_content.Length;

        _checksum = (ulong)content.GetHashCode();
    }

    #endregion

    #region Get-/Setters

    public ulong? Length { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new(_checksum);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.WriteAsync(_content.AsMemory());
    }

    #endregion

}
