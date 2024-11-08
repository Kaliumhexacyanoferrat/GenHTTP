using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Internal.Protocol;

public sealed class RequestQuery : PooledDictionary<string, string>, IRequestQuery
{
    private const int DefaultSize = 12;

    public RequestQuery() : base(DefaultSize, StringComparer.OrdinalIgnoreCase)
    {

    }

}
