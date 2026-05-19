namespace GenHTTP.Api.Protocol.Raw;

public interface IRawRequestTarget
{

    PathSegment? Current { get; }

    bool IsLast { get; }

    bool HasTrailingSlash { get; }

    void Advance(int segments = 1);

    string AsString(bool decode = true, bool remainingOnly = false);

}
