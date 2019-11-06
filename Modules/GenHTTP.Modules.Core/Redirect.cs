using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core
{

    public static class Redirect
    {

        public static RedirectProviderBuilder To(string location, bool temporary = false)
        {
            return new RedirectProviderBuilder().Location(location)
                                                .Mode(temporary);
        }

    }

}
