using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Utilities;

namespace GenHTTP.Engine.Protocol;

internal sealed class RequestQuery : PooledDictionary<string, string>, IRequestQuery
{
    private const int DefaultSize = 12;

    internal RequestQuery() : base(DefaultSize, StringComparer.OrdinalIgnoreCase)
    {

    }

}
