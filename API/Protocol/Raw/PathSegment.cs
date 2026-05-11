using System.Text;

namespace GenHTTP.Api.Protocol.Raw;

public readonly struct PathSegment
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

}
