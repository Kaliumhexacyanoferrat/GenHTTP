namespace GenHTTP.Api.Protocol.Raw;

public interface IRawRequestTarget
{

    PathSegment? Current { get; }

    bool IsLast { get; }

    void Advance(int segments = 1);

}
