﻿using System.Buffers;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol.Parser.Conversion
{

    internal static class ProtocolConverter
    {

        internal static HttpProtocol ToProtocol(ReadOnlySequence<byte> value)
        {
            var reader = new SequenceReader<byte>(value);

            if (value.Length != 8)
            {
                throw new ProtocolException("HTTP protocol version expected");
            }

            reader.Advance(5);

            var version = reader.Sequence.Slice(reader.Position);

            if (ValueConverter.CompareTo(version, "1.1"))
            {
                return HttpProtocol.Http_1_1;
            }
            else if (ValueConverter.CompareTo(version, "1.0"))
            {
                return HttpProtocol.Http_1_0;
            }

            var versionString = ValueConverter.GetString(version);

            throw new ProtocolException($"Unexpected protocol version '{versionString}'");
        }

    }

}
