namespace GenHTTP.Api.Protocol;

public interface IRequestTarget
{

    PathSegment? Current { get; }

    bool IsLast { get; }

    bool HasTrailingSlash { get; }

    void Advance(int segments = 1);

    PathSegment? Next(int offset);

    IRequestTarget CopyAndAppend(ReadOnlyMemory<byte> suffix);

    string AsString(bool decode = true, bool remainingOnly = false);

}
