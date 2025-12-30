namespace GenHTTP.Modules.Reflection.Routing;

/// <summary>
/// Passed as ref struct to routing segments to allow them
/// to add path argument values during matching.
/// </summary>
public struct PathArgumentSink
{
    
    internal Dictionary<string, string>? Arguments;

    public void Add(string key, string value)
        => (Arguments ??= new()).Add(key, value);
    
}
