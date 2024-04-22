using System.Buffers;
using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol.Parser.Conversion
{

    internal static class MethodConverter
    {
        private static readonly Dictionary<string, RequestMethod> KNOWN_METHODS = new(7)
        {
            { "GET",     RequestMethod.GET },
            { "HEAD",    RequestMethod.HEAD },
            { "POST",    RequestMethod.POST },
            { "PUT",     RequestMethod.PUT },
            { "PATCH",   RequestMethod.PATCH },
            { "DELETE",  RequestMethod.DELETE },
            { "OPTIONS", RequestMethod.OPTIONS }
        };

        internal static FlexibleRequestMethod ToRequestMethod(ReadOnlySequence<byte> value)
        {
            foreach (var kv in KNOWN_METHODS)
            {
                if (ValueConverter.CompareTo(value, kv.Key))
                {
                    return FlexibleRequestMethod.Get(kv.Value);
                }
            }

            return FlexibleRequestMethod.Get(ValueConverter.GetString(value));
        }

    }

}
