using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public class RetainedRequestHeader : IRequestHeader
{

    public HttpProtocol Protocol { get; }

    public RequestMethod Method { get; }

    public ReadOnlyMemory<byte> Path { get; }

    public IRequestTarget Target { get; }

    public IKeyValueList Query { get; }

    public IKeyValueList Headers { get; }

    internal RetainedRequestHeader(RequestHeader source)
    {
        Path = source.Path.ToArray();

        Protocol = new(source.Version.ToArray());
        Method = new(source.Method.Value.ToArray());

        var target = new RequestTarget();

        target.Apply(Path);

        Target = target;

        Query = new RetainedKeyValueList(source.Query);
        Headers = new RetainedKeyValueList(source.Headers);
    }

}
