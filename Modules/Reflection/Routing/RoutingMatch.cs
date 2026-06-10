using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Routing;

public readonly struct RoutingMatch
{

    public int Offset { get; }

    public IReadOnlyDictionary<ByteString, ByteString>? PathArguments { get; }

    /// <summary>
    /// Returned by the operation router if a web service method matched
    /// the incoming request.
    /// </summary>
    /// <param name="Offset">The offset to advance the request target by</param>
    /// <param name="PathArguments">The arguments read from the request path, if any</param>
    public RoutingMatch(int offset, IReadOnlyDictionary<ByteString, ByteString>? pathArguments)
    {
        Offset = offset;
        PathArguments = pathArguments;
    }

}
