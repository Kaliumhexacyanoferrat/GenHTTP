using System.Globalization;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Pooling;

namespace GenHTTP.Modules.IO.Formattable;

public sealed class FormattableContent : PooledContent<FormattableContent>, IResponseContent
{
    private static readonly IUtf8SpanFormattable NoValue = 0;

    private IUtf8SpanFormattable _value = NoValue;

    private int _length;

    internal static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    
    #region Get-/Setters
    
    public ulong? Length => (ulong)_length;

    public ContentType? Type => ContentType.TextPlain;

    public ReadOnlyMemory<byte>? Encoding => null;
    
    #endregion
    
    #region Initialization

    public FormattableContent(IUtf8SpanFormattable value)
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
    /// request.Respond().Content(FormattableContent.Rent(DateOnly.FromDateTime(DateTime.Now))).Build();
    /// </code>
    /// </example>
    public static FormattableContent Rent(IUtf8SpanFormattable value)
    {
        var content = FromPool(static () => new FormattableContent());

        content.Apply(value);

        return content;
    }

    private void Apply(IUtf8SpanFormattable value)
    {
        _value = value;

        Span<byte> scratch = stackalloc byte[64];

        if (!_value.TryFormat(scratch, out _length, default, Invariant))
        {
            throw new InvalidOperationException($"Failed to format value of type {value.GetType()}.");
        }
    }
    
    #endregion
    
    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)_value.GetHashCode());

    public ValueTask WriteAsync(IResponseSink sink)
    {
        var writer = sink.Writer;

        var span = writer.GetSpan(_length);

        _value.TryFormat(span, out _, default, Invariant);

        writer.Advance(_length);

        return ValueTask.CompletedTask;
    }

    protected override void Reset()
    {
        _value = NoValue;
        _length = 0;
    }

    #endregion
    
}
