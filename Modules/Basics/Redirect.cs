using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics.Providers;

namespace GenHTTP.Modules.Basics
{

    public static class Redirect
    {

        public static RedirectProviderBuilder To(IRequest request, string path, bool temporary = false)
        {
            var protocol = request.EndPoint.Secure ? "https://" : "http://";

            return To($"{protocol}{request.Host}{path}", temporary);
        }

        public static RedirectProviderBuilder To(string location, bool temporary = false)
        {
            return new RedirectProviderBuilder().Location(location)
                                                .Mode(temporary);
        }

    }

}
