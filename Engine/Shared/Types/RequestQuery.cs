using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class RequestQuery : PooledDictionary<string, string>, IRequestQuery
{
    private const int DefaultSize = 12;

    public RequestQuery() : base(DefaultSize, StringComparer.OrdinalIgnoreCase)
    {

    }

}
