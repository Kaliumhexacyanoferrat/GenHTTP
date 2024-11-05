using GenHTTP.Api.Protocol;

using Microsoft.AspNetCore.Http;

namespace GenHTTP.Engine.Kestrel.Types;

public class Cookies : Dictionary<string, Cookie>, ICookieCollection
{

    public Cookies(HttpRequest request)
    {

    }

    public void Dispose()
    {

    }

}
