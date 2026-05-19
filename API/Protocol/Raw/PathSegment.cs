using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace GenHTTP.Api.Protocol.Raw;

[DebuggerDisplay("{DebuggerValue,nq}")]
public readonly struct PathSegment : IEquatable<PathSegment>
{

    public readonly ReadOnlyMemory<byte> Encoded;

    #region Initialization

    public PathSegment(ReadOnlyMemory<byte> encoded)
    {
        Encoded = encoded;
    }

    public PathSegment(string decoded)
    {
        Encoded = new ReadOnlyMemory<byte>(Encoding.ASCII.GetBytes(Uri.EscapeDataString(decoded)));
    }

    #endregion

    #region Functionality

    public string Decode()
    {
        var span = Encoded.Span;

        if (span.IndexOf((byte)'%') < 0)
        {
            return Encoding.ASCII.GetString(span);
        }

        byte[]? rented = null;

        var buffer = span.Length <= 256 ? stackalloc byte[span.Length] : (rented = ArrayPool<byte>.Shared.Rent(span.Length));

        try
        {
            int write = 0, i = 0;

            while (i < span.Length)
            {
                if (span[i] == '%' && i + 2 < span.Length && IsHexDigit(span[i + 1]) && IsHexDigit(span[i + 2]))
                {
                    buffer[write++] = (byte)((HexValue(span[i + 1]) << 4) | HexValue(span[i + 2]));
                    i += 3;
                }
                else
                {
                    buffer[write++] = span[i++];
                }
            }

            return Encoding.UTF8.GetString(buffer[..write]);
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    private static bool IsHexDigit(byte b) => (uint)(b - '0') <= 9 || (uint)((b | 0x20) - 'a') <= 5;

    private static int HexValue(byte b) => b <= '9' ? b - '0' : (b | 0x20) - 'a' + 10;

    #endregion

    #region Equality

    public bool Equals(PathSegment other) => Encoded.Span.SequenceEqual(other.Encoded.Span);

    public override bool Equals(object? obj) => obj is PathSegment other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.AddBytes(Encoded.Span);
        return hash.ToHashCode();
    }

    public static bool operator ==(PathSegment left, PathSegment right) => left.Equals(right);

    public static bool operator !=(PathSegment left, PathSegment right) => !left.Equals(right);

    #endregion

    #region Debugging support

    private string DebuggerValue => Encoding.ASCII.GetString(Encoded.Span);

    #endregion

}
