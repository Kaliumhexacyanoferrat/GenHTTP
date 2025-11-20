using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Internal.Protocol;

public sealed class RequestQuery : Dictionary<string, string>, IRequestQuery
{
    private const int DefaultSize = 12;

    public RequestQuery() : base(DefaultSize, StringComparer.OrdinalIgnoreCase)
    {

    }

}
