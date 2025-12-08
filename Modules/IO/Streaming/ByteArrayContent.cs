using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Streaming;

/// <summary>
/// Response content backed by a byte array.
/// </summary>
public sealed class ByteArrayContent : IResponseContent
{
    private readonly byte[] _content;

    private readonly ChecksumProvider _checksumProvider;

    #region Initialization

    public ByteArrayContent(byte[] content, Func<ValueTask<ulong?>>? checksumProvider)
    {
        _content = content;

        Length = (ulong)_content.Length;

        _checksumProvider = new ChecksumProvider(checksumProvider ?? (() => new(CalculateChecksum(content))));
    }

    #endregion

    #region Get-/Setters

    public ulong? Length { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => _checksumProvider.Compute();

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.WriteAsync(_content.AsMemory());
    }

    private static ulong CalculateChecksum(byte[] content)
    {
        unchecked
        {
            ulong hash = 17;

            for (var i = 0; i < content.Length; i++)
            {
                hash = hash * 23 + content[i];
            }

            return hash;
        }
    }

    #endregion
}
