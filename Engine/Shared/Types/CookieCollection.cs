using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class CookieCollection : PooledDictionary<string, Cookie>, ICookieCollection
{
    private const int DefaultSize = 6;

    public CookieCollection() : base(DefaultSize, StringComparer.InvariantCultureIgnoreCase)
    {

    }

    internal void Add(string header)
    {
        foreach (var cookie in Parse(header))
        {
            if (!ContainsKey(cookie.Name))
            {
                Add(cookie.Name, cookie);
            }
        }
    }

    private static List<Cookie> Parse(string value)
    {
        var result = new List<Cookie>(2);

        var cookies = value.Split("; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        foreach (var kv in cookies)
        {
            var index = kv.IndexOf('=');

            if (index > -1)
            {
                result.Add(new Cookie(kv[..index], kv[(index + 1)..]));
            }
        }

        return result;
    }
}
