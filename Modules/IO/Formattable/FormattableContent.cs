using System.Globalization;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Formattable;

public sealed class FormattableContent : IResponseContent
{
    private readonly IUtf8SpanFormattable _value;

    private readonly int _length;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public ulong? Length => (ulong)_length;

    public ContentType? Type => ContentType.TextPlain;

    public ReadOnlyMemory<byte>? Encoding => null;

    public FormattableContent(IUtf8SpanFormattable value)
    {
        _value = value;

        Span<byte> scratch = stackalloc byte[64];

        if (!_value.TryFormat(scratch, out _length, default, Invariant))
        {
            throw new InvalidOperationException($"Failed to format value of type {value.GetType()}.");
        }
    }

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)_value.GetHashCode());

    public ValueTask WriteAsync(IResponseSink sink)
    {
        var writer = sink.Writer;

        var span = writer.GetSpan(_length);

        _value.TryFormat(span, out _, default, Invariant);

        writer.Advance(_length);

        return ValueTask.CompletedTask;
    }

}
