namespace GenHTTP.Modules.Reflection.Routing;

public struct PathArgumentSink
{
    
    internal Dictionary<string, string>? Arguments;

    public void Add(string key, string value)
        => (Arguments ??= new()).Add(key, value);
    
}
