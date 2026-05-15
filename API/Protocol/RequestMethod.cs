using System.Text;

namespace GenHTTP.Api.Protocol;

public readonly struct RequestMethod : IEquatable<RequestMethod>
{
    private readonly ReadOnlyMemory<byte> _value;

    private readonly int _hashCode;

    public RequestMethod(string type) : this(Encoding.ASCII.GetBytes(type)) { }

    public RequestMethod(ReadOnlyMemory<byte> value)
    {
        _value = value;

        var hash = new HashCode();
        hash.AddBytes(value.Span);

        _hashCode = hash.ToHashCode();
    }

    public ReadOnlyMemory<byte> Value => _value;

    #region Known Methods

    public static readonly RequestMethod Get = new("GET"u8.ToArray());

    public static readonly RequestMethod Head = new("HEAD"u8.ToArray());

    public static readonly RequestMethod Post = new("POST"u8.ToArray());

    public static readonly RequestMethod Put = new("PUT"u8.ToArray());

    public static readonly RequestMethod Delete = new("DELETE"u8.ToArray());

    public static readonly RequestMethod Connect = new("CONNECT"u8.ToArray());

    public static readonly RequestMethod Options = new("OPTIONS"u8.ToArray());

    public static readonly RequestMethod Trace = new("TRACE"u8.ToArray());

    public static readonly RequestMethod Patch = new("PATCH"u8.ToArray());

    #endregion

    #region Equality and Hashing

    public bool Equals(RequestMethod other)
        => _value.Span.SequenceEqual(other._value.Span);

    public override bool Equals(object? obj)
        => obj is RequestMethod other && Equals(other);

    public override int GetHashCode() => _hashCode;

    public static bool operator ==(RequestMethod left, RequestMethod right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RequestMethod left, RequestMethod right)
    {
        return !(left == right);
    }

    #endregion

}
