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

        byte[]? rented = null;

        var buffer = span.Length <= 256 ? stackalloc byte[span.Length] : (rented = ArrayPool<byte>.Shared.Rent(span.Length));

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

    #endregion

}
