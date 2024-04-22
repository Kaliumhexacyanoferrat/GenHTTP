using System.Buffers;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol.Parser.Conversion
{

    internal static class MethodConverter
    {

        internal static FlexibleRequestMethod ToRequestMethod(ReadOnlySequence<byte> value)
        {
            if (ValueConverter.CompareTo(value, "GET")) return FlexibleRequestMethod.Get(RequestMethod.GET);

            return FlexibleRequestMethod.Get(ValueConverter.GetString(value));
        }

    }

}
