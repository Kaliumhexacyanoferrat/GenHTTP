using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Utilities;

namespace GenHTTP.Engine.Internal.Protocol;

internal sealed class RequestQuery : PooledDictionary<string, string>, IRequestQuery
{
    private const int DefaultSize = 12;

    internal RequestQuery() : base(DefaultSize, StringComparer.OrdinalIgnoreCase)
    {

    }
}
