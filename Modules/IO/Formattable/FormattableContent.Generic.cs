using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Formattable;

public sealed class FormattableContent<T> : IResponseContent where T : struct, IUtf8SpanFormattable
{
    private T _value;

    private readonly int _length;
    
    #region Get-/Setters
    
    public ulong? Length => (ulong)_length;

    public ContentType? Type => ContentType.TextPlain;

    public ReadOnlyMemory<byte>? Encoding => null;
    
    #endregion
    
    #region Initialization

    public FormattableContent(T value)
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

    #endregion
    
}
