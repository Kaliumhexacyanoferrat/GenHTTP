using System.Buffers;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol.Parser.Conversion;

internal static class ProtocolConverter
{

    internal static HttpProtocol ToProtocol(ReadOnlySequence<byte> value)
    {
        var reader = new SequenceReader<byte>(value);

        if (value.Length != 8)
        {
            throw new ProtocolException($"HTTP protocol version expected (got: '{ValueConverter.GetString(value)}')");
        }

        reader.Advance(5);

        var version = reader.Sequence.Slice(reader.Position);

        if (ValueConverter.CompareTo(version, "1.1"))
        {
            return HttpProtocol.Http11;
        }
        if (ValueConverter.CompareTo(version, "1.0"))
        {
            return HttpProtocol.Http10;
        }

        var versionString = ValueConverter.GetString(version);

        throw new ProtocolException($"Unexpected protocol version '{versionString}'");
    }
}
