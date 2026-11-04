using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Redirects;
using GenHTTP.Modules.VirtualHosting;

namespace GenHTTP.Playground.Samples;

public static class VirtualHostingSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Hosts multiple websites on a single server, one per domain. Browsers
         * resolve *.localhost to your machine, so you can try this locally:
         *
         *   http://blog.localhost:8080/virtual-hosting/
         *   http://shop.localhost:8080/virtual-hosting/
         *   http://localhost:8080/virtual-hosting/  (redirects to the blog)
         *
         * See https://genhttp.org/documentation/content/handlers/virtual-hosts/
         *
         */

        var blog = Content.From(Resource.FromString("<h1>My Blog</h1>").Type(ContentType.TextHtml));

        var shop = Content.From(Resource.FromString("<h1>My Shop</h1>").Type(ContentType.TextHtml));

        return VirtualHosts.Create()
                           .Add("blog.localhost", blog)
                           .Add("shop.localhost", shop)
                           .Default(Redirect.To("http://blog.localhost:8080/virtual-hosting/", temporary: true)); // any other host
    }

}
