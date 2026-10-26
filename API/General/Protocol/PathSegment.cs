using System.Buffers;
using System.Text;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// A single percent-encoded path segment.
/// </summary>
[MemoryView]
public readonly partial struct PathSegment
{
    private static readonly Encoding Ascii = Encoding.ASCII;
    
    private static readonly Encoding Utf8 = Encoding.UTF8;

    #region Functionality

    /// <summary>Percent-decodes this segment and returns the human-readable string.</summary>
    public string Decode()
    {
        var span = Bytes.Span;

        if (span.IndexOf((byte)'%') < 0)
        {
            return Ascii.GetString(span);
        }

        var rented = span.Length > 256 ? ArrayPool<byte>.Shared.Rent(span.Length) : null;

        var buffer = rented != null ? rented : stackalloc byte[span.Length];

        try
        {
            var written = PercentEncoding.Decode(span, buffer);

            return Utf8.GetString(buffer[..written]);
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    /// <summary>
    /// Percent-decodes this segment into the given buffer without allocating a string.
    /// </summary>
    /// <param name="target">The buffer to write to (must provide at least as many chars as the segment has bytes)</param>
    /// <returns>The number of chars written to <paramref name="target"/></returns>
    public int Decode(Span<char> target)
    {
        var span = Bytes.Span;

        if (span.IndexOf((byte)'%') < 0)
        {
            return Ascii.GetChars(span, target);
        }

        var rented = span.Length > 256 ? ArrayPool<byte>.Shared.Rent(span.Length) : null;

        var buffer = rented != null ? rented : stackalloc byte[span.Length];

        try
        {
            var written = PercentEncoding.Decode(span, buffer);

            return Utf8.GetChars(buffer[..written], target);
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    #endregion

}
