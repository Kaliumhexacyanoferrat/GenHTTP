using System.Buffers;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol.Parser.Conversion;

internal static class MethodConverter
{
    private static readonly Dictionary<string, RequestMethod> KnownMethods = new(7)
    {
        {
            "GET", RequestMethod.Get
        },
        {
            "HEAD", RequestMethod.Head
        },
        {
            "POST", RequestMethod.Post
        },
        {
            "PUT", RequestMethod.Put
        },
        {
            "PATCH", RequestMethod.Patch
        },
        {
            "DELETE", RequestMethod.Delete
        },
        {
            "OPTIONS", RequestMethod.Options
        }
    };

    internal static FlexibleRequestMethod ToRequestMethod(ReadOnlySequence<byte> value)
    {
        foreach (var kv in KnownMethods)
        {
            if (ValueConverter.CompareTo(value, kv.Key))
            {
                return FlexibleRequestMethod.Get(kv.Value);
            }
        }

        return FlexibleRequestMethod.Get(ValueConverter.GetString(value));
    }
}
