using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Redirects;
using GenHTTP.Modules.ReverseProxy;

namespace GenHTTP.Playground.Samples;

public static class LegacyMigrationSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Replaces a legacy web application step by step: GenHTTP sits in front of
         * the old application and serves the parts that have already been rewritten.
         * Moved pages are redirected, everything else is still served by the legacy
         * application (expected to run on http://localhost:5000).
         *
         * See https://genhttp.org/documentation/content/handlers/reverse-proxy/
         * and https://genhttp.org/documentation/content/handlers/redirects/
         *
         */

        var newApi = Inline.Create()
                           .Get("status", () => new Status(true));

        var contact = Content.From(Resource.FromString("<h1>Contact us</h1>").Type(ContentType.TextHtml));

        var legacy = Proxy.Create()
                          .Upstream("http://localhost:5000/")
                          .ConnectTimeout(TimeSpan.FromSeconds(5))
                          .ReadTimeout(TimeSpan.FromSeconds(30));

        return Layout.Create()
                     .Add("api/v2", newApi) // GET http://localhost:8080/legacy-migration/api/v2/status
                     .Add("contact", contact)
                     .Add("contact.php", Redirect.To("/legacy-migration/contact")) // permanent, so search engines learn the new URL
                     .Add("shop", Redirect.To("https://shop.example.com/", temporary: true))
                     .Add(legacy); // everything else, e.g. GET http://localhost:8080/legacy-migration/login.php
    }

    public record Status(bool Healthy);

}
