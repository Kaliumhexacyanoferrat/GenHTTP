using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Routing;

/// <summary>
/// Passed as ref struct to routing segments to allow them
/// to add path argument values during matching.
/// </summary>
public struct PathArgumentSink
{

    internal Dictionary<ByteString, ByteString>? Arguments;

    public void Add(ByteString key, ByteString value)
        => (Arguments ??= new()).Add(key, value);

}
