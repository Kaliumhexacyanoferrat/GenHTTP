using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Strings;

public sealed class StringContent : IResponseContent
{
    private static readonly Encoding UTF8 = Encoding.UTF8;

    private readonly byte[] _Content;

    private readonly ulong _Checksum;

    #region Get-/Setters

    public ulong? Length { get; }

    #endregion

    #region Initialization

    public StringContent(string content) 
    { 
            _Content = UTF8.GetBytes(content);

            Length = (ulong)_Content.Length;

            _Checksum = (ulong)content.GetHashCode();
        }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new(_Checksum);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
            await target.WriteAsync(_Content.AsMemory());
        }

    #endregion

}
