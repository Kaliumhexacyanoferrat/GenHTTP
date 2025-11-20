using GenHTTP.Api.Protocol;

using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore.Types;

public sealed class Cookies : Dictionary<string, Cookie>, ICookieCollection
{

    public void SetRequest(HttpRequest request)
    {
        foreach (var cookie in request.Cookies)
        {
            Add(cookie.Key, new(cookie.Key, cookie.Value));
        }
    }

}
