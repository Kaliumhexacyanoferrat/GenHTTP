using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types.Raw;

public class RetainedRequestHeader : IRawRequestHeader
{

    public RequestMethod Method { get; }

    public ReadOnlyMemory<byte> Path { get; }

    public IRawRequestTarget Target { get; }

    public IRawKeyValueList Query { get; }

    public ReadOnlyMemory<byte> Version { get; }

    public IRawKeyValueList Headers { get; }

    internal RetainedRequestHeader(RawRequestHeader source)
    {
        Path = source.Path.ToArray();
        Version = source.Version.ToArray();

        Method = new(source.Method.Value.ToArray());

        var target = new RawRequestTarget();

        target.Apply(Path);

        Target = target;

        Query = new RetainedKeyValueList(source.Query);
        Headers = new RetainedKeyValueList(source.Headers);
    }

}
