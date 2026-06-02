using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Routing;

/// <summary>
/// Passed as ref struct to routing segments to allow them
/// to add path argument values during matching.
/// </summary>
public struct PathArgumentSink
{

    internal Dictionary<ArgumentName, ReadOnlyMemory<byte>>? Arguments;

    public void Add(ArgumentName key, ReadOnlyMemory<byte> value)
        => (Arguments ??= new()).Add(key, value);

}
