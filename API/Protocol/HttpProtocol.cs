using System.Text;

namespace GenHTTP.Api.Protocol;

public readonly struct HttpProtocol : IEquatable<HttpProtocol>
{
    private readonly ReadOnlyMemory<byte> _value;

    private readonly int _hashCode;

    public HttpProtocol(string version) : this(Encoding.ASCII.GetBytes(version)) { }

    public HttpProtocol(ReadOnlyMemory<byte> value)
    {
        _value = value;

        var hash = new HashCode();
        hash.AddBytes(value.Span);

        _hashCode = hash.ToHashCode();
    }

    public ReadOnlyMemory<byte> Value => _value;

    #region Known Methods

    public static readonly HttpProtocol Http10 = new("HTTP/1.0"u8.ToArray());

    public static readonly HttpProtocol Http11 = new("HTTP/1.1"u8.ToArray());

    // well, todo

    public static readonly HttpProtocol Http2 = new("HTTP/2.0"u8.ToArray());

    public static readonly HttpProtocol Http3 = new("HTTP/3.0"u8.ToArray());

    #endregion

    #region Equality and Hashing

    public bool Equals(HttpProtocol other)
        => _value.Span.SequenceEqual(other._value.Span);

    public override bool Equals(object? obj)
        => obj is HttpProtocol other && Equals(other);

    public override int GetHashCode() => _hashCode;

    public static bool operator ==(HttpProtocol left, HttpProtocol right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HttpProtocol left, HttpProtocol right)
    {
        return !(left == right);
    }

    public override string ToString() => Encoding.ASCII.GetString(Value.Span);

    #endregion

}
