using System.Text;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Strings;

public sealed class StringContent : IResponseContent
{
    private static readonly Encoding Utf8 = Encoding.UTF8;

    private readonly ulong _Checksum;

    private readonly byte[] _Content;

    #region Initialization

    public StringContent(string content)
    {
        _Content = Utf8.GetBytes(content);

        Length = (ulong)_Content.Length;

        _Checksum = (ulong)content.GetHashCode();
    }

    #endregion

    #region Get-/Setters

    public ulong? Length { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new(_Checksum);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.WriteAsync(_Content.AsMemory());
    }

    #endregion

}
