using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Routing;

/// <summary>
/// Passed as ref struct to routing segments to allow them
/// to add path argument values during matching.
/// </summary>
/// <remarks>
/// Values are stored by position, in the order of
/// <see cref="OperationRoute.Arguments" />.
/// </remarks>
public struct PathArgumentSink
{
    private readonly int _capacity;

    internal ByteString[]? Values;

    private int _count;

    internal PathArgumentSink(int capacity)
    {
        _capacity = capacity;
    }

    /// <summary>
    /// Adds the value of the next path argument declared by the route.
    /// </summary>
    /// <param name="value">The raw value read from the path</param>
    public void Add(ByteString value)
    {
        if (_count >= _capacity)
        {
            throw new InvalidOperationException("Routing segments provided more values than they declared");
        }

        (Values ??= new ByteString[_capacity])[_count++] = new(PercentEncoding.Decode(value.Bytes));
    }

}
