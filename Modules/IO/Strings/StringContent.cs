using System.Text.Unicode;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Pooling;

namespace GenHTTP.Modules.IO.Strings;

public sealed class StringContent : PooledContent<StringContent>, IResponseContent
{
    private const int MaxChunkSize = 16384;

    private string _content = string.Empty;

    private ulong _length;

    private ContentType _type;

    #region Initialization

    public StringContent(string content, ContentType? contentType = null)
    {
        Apply(content, contentType);
    }

    private StringContent() { }

    /// <summary>
    /// Fetches a pooled instance that renders the given text.
    /// </summary>
    /// <remarks>
    /// The instance is returned to the pool when the server disposes it after
    /// writing the response, so it must be passed to exactly one response.
    /// </remarks>
    /// <param name="content">The text to be written</param>
    /// <param name="contentType">The content type of the text (defaults to text/plain)</param>
    /// <returns>The pooled content rendering the given text</returns>
    /// <example>
    /// <code>
    /// request.Respond().Content(StringContent.Rent("Hello World!")).Build();
    /// </code>
    /// </example>
    public static StringContent Rent(string content, ContentType? contentType = null)
    {
        var result = FromPool(static () => new StringContent());

        result.Apply(content, contentType);

        return result;
    }

    private void Apply(string content, ContentType? contentType)
    {
        _content = content;
        _length = (ulong)System.Text.Encoding.UTF8.GetByteCount(content);
        _type = contentType ?? ContentType.TextPlain;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => _length;

    public ContentType? Type => _type;

    public ReadOnlyMemory<byte>? Encoding => null;

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)_content.GetHashCode());

    public ValueTask WriteAsync(IResponseSink sink)
    {
        var writer = sink.Writer;

        var remaining = _content.AsSpan();

        while (!remaining.IsEmpty)
        {
            var target = writer.GetSpan(Math.Min(remaining.Length * 3, MaxChunkSize));

            Utf8.FromUtf16(remaining, target, out var read, out var written);

            writer.Advance(written);

            remaining = remaining[read..];
        }

        return ValueTask.CompletedTask;
    }

    protected override void Reset()
    {
        _content = string.Empty;
        _length = 0;
        _type = default;
    }

    #endregion

}
