using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Streaming;

/// <summary>
/// Response content backed by a ReadOnlyMemory of bytes.
/// </summary>
public sealed class MemoryContent : IResponseContent
{
    private readonly ReadOnlyMemory<byte> _content;

    private readonly ulong _checksum;

    #region Initialization

    public MemoryContent(ReadOnlyMemory<byte> content)
    {
        _content = content;

        Length = (ulong)_content.Length;

        _checksum = CalculateChecksum(content.Span);
    }

    #endregion

    #region Get-/Setters

    public ulong? Length { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new(_checksum);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.WriteAsync(_content);
    }

    private static ulong CalculateChecksum(ReadOnlySpan<byte> content)
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
