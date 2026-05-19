using System.Text;

using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Api.Content.IO;

public class AlgorithmName
{

    public readonly ReadOnlyMemory<byte> Value;

    #region Initialization

    public AlgorithmName(ReadOnlyMemory<byte> value)
    {
        Value = value;
    }

    public AlgorithmName(string stringValue)
    {
        Value = new ReadOnlyMemory<byte>(Encoding.ASCII.GetBytes(stringValue));
    }

    #endregion

    #region Equality

    public bool Equals(PathSegment other) => Value.Span.SequenceEqual(other.Encoded.Span);

    public override bool Equals(object? obj) => obj is PathSegment other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.AddBytes(Value.Span);
        return hash.ToHashCode();
    }

    public static bool operator ==(AlgorithmName left, AlgorithmName right) => left.Equals(right);

    public static bool operator !=(AlgorithmName left, AlgorithmName right) => !left.Equals(right);

    #endregion

}
