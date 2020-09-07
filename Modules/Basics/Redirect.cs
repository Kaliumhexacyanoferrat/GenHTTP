using GenHTTP.Modules.Basics.Providers;

namespace GenHTTP.Modules.Basics
{

    public static class Redirect
    {

        public static RedirectProviderBuilder To(string location, bool temporary = false) => new RedirectProviderBuilder().Location(location)
                                                                                                                          .Mode(temporary);

    }

}
