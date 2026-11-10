using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Routing;

public readonly struct RoutingMatch
{
    private readonly ByteString[]? _pathArguments;

    public int Offset { get; }

    /// <summary>
    /// Returned by the operation router if a web service method matched
    /// the incoming request.
    /// </summary>
    /// <param name="offset">The offset to advance the request target by</param>
    /// <param name="pathArguments">The values read from the request path, in the order of <see cref="OperationRoute.Arguments" /></param>
    public RoutingMatch(int offset, ByteString[]? pathArguments)
    {
        Offset = offset;
        _pathArguments = pathArguments;
    }

    /// <summary>
    /// Reads the value of a path argument.
    /// </summary>
    /// <param name="index">The position of the argument in <see cref="OperationRoute.Arguments" /></param>
    /// <param name="value">The value read from the request path</param>
    /// <returns>true, if the request provided a value for this argument</returns>
    public bool TryGetPathArgument(int index, out ByteString value)
    {
        var arguments = _pathArguments;

        if (arguments != null && (uint)index < (uint)arguments.Length)
        {
            value = arguments[index];
            return true;
        }

        value = default;
        return false;
    }

}
