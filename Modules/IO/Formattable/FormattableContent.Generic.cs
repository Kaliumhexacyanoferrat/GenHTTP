using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Pooling;

namespace GenHTTP.Modules.IO.Formattable;

public sealed class FormattableContent<T> : PooledContent<FormattableContent<T>>, IResponseContent where T : struct, IUtf8SpanFormattable
{
    private T _value;

    private int _length;
    
    #region Get-/Setters
    
    public ulong? Length => (ulong)_length;

    public ContentType? Type => ContentType.TextPlain;

    public ReadOnlyMemory<byte>? Encoding => null;
    
    #endregion
    
    #region Initialization

    public FormattableContent(T value)
    {
        Apply(value);
    }

    private FormattableContent() { }

    /// <summary>
    /// Fetches a pooled instance that renders the given value.
    /// </summary>
    /// <remarks>
    /// The instance is returned to the pool when the server disposes it after
    /// writing the response, so it must be passed to exactly one response.
    /// </remarks>
    /// <param name="value">The value to be written</param>
    /// <returns>The pooled content rendering the given value</returns>
    /// <example>
    /// <code>
    /// request.Respond().Content(FormattableContent&lt;int&gt;.Rent(42)).Build();
    /// </code>
    /// </example>
    public static FormattableContent<T> Rent(T value)
    {
        var content = FromPool(static () => new FormattableContent<T>());

        content.Apply(value);

        return content;
    }

    private void Apply(T value)
    {
        _value = value;

        Span<byte> scratch = stackalloc byte[64];

        if (!_value.TryFormat(scratch, out _length, default, FormattableContent.Invariant))
        {
            throw new InvalidOperationException($"Failed to format value of type {typeof(T)}.");
        }
    }

    #endregion
    
    #region Functionality
    
    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)_value.GetHashCode());

    public ValueTask WriteAsync(IResponseSink sink)
    {
        var writer = sink.Writer;

        var span = writer.GetSpan(_length);

        _value.TryFormat(span, out _, default, FormattableContent.Invariant);

        writer.Advance(_length);

        return ValueTask.CompletedTask;
    }

    protected override void Reset()
    {
        _value = default;
        _length = 0;
    }

    #endregion
    
}
