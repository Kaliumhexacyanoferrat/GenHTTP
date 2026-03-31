using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types.Raw;

public class RetainedRequestHeader : IRawRequestHeader
{
    
    public ReadOnlyMemory<byte> Method { get; }
    
    public ReadOnlyMemory<byte> Path { get; }
    
    public IRawRequestTarget Target { get; }
    
    public IRawKeyValueList Query { get; }
    
    public ReadOnlyMemory<byte> Version { get; }
    
    public IRawKeyValueList Headers { get; }
    
    internal RetainedRequestHeader(RawRequestHeader source)
    {
        Method = source.Method.ToArray();
        Path = source.Path.ToArray();
        Version = source.Version.ToArray();

        var target = new RawRequestTarget();
        
        target.Apply(Path);

        Target = target;
        
        Query = new RetainedKeyValueList(source.Query);
        Headers = new RetainedKeyValueList(source.Headers);
    }

}
