using System.Buffers;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Streaming;

/// <summary>
/// Response content backed by a ReadOnlyMemory of bytes.
/// </summary>
public sealed class MemoryContent : IResponseContent
{
    private readonly ReadOnlyMemory<byte> _content;

    private readonly ChecksumProvider _checksumProvider;

    #region Initialization

    public MemoryContent(ReadOnlyMemory<byte> content, ContentType contentType, Func<ValueTask<ulong?>>? checksumProvider)
    {
        _content = content;

        Length = (ulong)_content.Length;
        Type = contentType;

        _checksumProvider = new ChecksumProvider(checksumProvider ?? (() => new(CalculateChecksum(content.Span))));
    }

    #endregion

    #region Get-/Setters

    public ulong? Length { get; }

    public ContentType? Type { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => _checksumProvider.Compute();

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.WriteAsync(_content);
    }

    public ValueTask WriteAsync(IResponseSink sink)
    {
        sink.Writer.Write(_content.Span);
        return default;
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
