namespace GenHTTP.Api.Protocol.Raw;

public interface IRawRequestTarget
{

    PathSegment? Current { get; }

    void Advance(int segments = 1);

}
