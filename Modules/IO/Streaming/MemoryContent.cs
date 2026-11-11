using System.Buffers;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Pooling;

namespace GenHTTP.Modules.IO.Streaming;

/// <summary>
/// Response content backed by a ReadOnlyMemory of bytes.
/// </summary>
public sealed class MemoryContent : PooledContent<MemoryContent>, IResponseContent
{
    private ReadOnlyMemory<byte> _content = ReadOnlyMemory<byte>.Empty;

    private Func<ValueTask<ulong?>>? _checksumProvider;

    #region Initialization

    public MemoryContent(ReadOnlyMemory<byte> content, ContentType? contentType, Func<ValueTask<ulong?>>? checksumProvider)
    {
        Apply(content, contentType, checksumProvider);
    }

    private MemoryContent() { }

    /// <summary>
    /// Fetches a pooled instance that serves the given memory.
    /// </summary>
    /// <remarks>
    /// The instance is returned to the pool when the server disposes it after
    /// writing the response, so it must be passed to exactly one response.
    /// </remarks>
    /// <param name="content">The data to be served</param>
    /// <param name="contentType">The content type of the data (defaults to application/octet-stream)</param>
    /// <param name="checksumProvider">Calculates the checksum of the data (computed from the data, if not given)</param>
    /// <returns>The pooled content serving the given memory</returns>
    /// <example>
    /// <code>
    /// request.Respond().Content(MemoryContent.Rent(data, ContentType.ApplicationOctetStream, null)).Build();
    /// </code>
    /// </example>
    public static MemoryContent Rent(ReadOnlyMemory<byte> content, ContentType? contentType, Func<ValueTask<ulong?>>? checksumProvider)
    {
        var result = FromPool(static () => new MemoryContent());

        result.Apply(content, contentType, checksumProvider);

        return result;
    }

    private void Apply(ReadOnlyMemory<byte> content, ContentType? contentType, Func<ValueTask<ulong?>>? checksumProvider)
    {
        _content = content;

        Length = (ulong)content.Length;
        Type = contentType ?? ContentType.ApplicationOctetStream;

        _checksumProvider = checksumProvider;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length { get; private set; }

    public ContentType? Type { get; private set; }

    public ReadOnlyMemory<byte>? Encoding => null;

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => (_checksumProvider != null) ? _checksumProvider() : new(CalculateChecksum(_content.Span));

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.WriteAsync(_content);
    }

    public ValueTask WriteAsync(IResponseSink sink)
    {
        sink.Writer.Write(_content.Span);
        return default;
    }

    protected override void Reset()
    {
        _content = ReadOnlyMemory<byte>.Empty;
        _checksumProvider = null;

        Length = 0;
        Type = null;
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
