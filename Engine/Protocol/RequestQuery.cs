using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Utilities;

namespace GenHTTP.Engine.Protocol;

internal sealed class RequestQuery : PooledDictionary<string, string>, IRequestQuery
{
    private const int DEFAULT_SIZE = 12;

    internal RequestQuery() : base(DEFAULT_SIZE, StringComparer.OrdinalIgnoreCase)
    {

        }

}
