using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public class RetainedRequestHeader : IRequestHeader
{

    public HttpProtocol Protocol { get; }

    public RequestMethod Method { get; }

    public ByteString Path { get; }

    public IRequestTarget Target { get; }

    public IRequestQuery Query { get; }

    public IRequestHeaders Headers { get; }

    internal RetainedRequestHeader(RequestHeader source)
    {
        Path = new(source.Path.Bytes.ToArray());

        Protocol = new(source.Version.ToArray());
        Method = new(source.Method.Bytes.ToArray());

        var target = new RequestTarget();

        target.Apply(Path);

        Target = target;

        Query = new RetainedKeyValueList(source.Query);
        Headers = new RetainedKeyValueList(source.Headers);
    }

}
