using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core;

namespace GenHTTP.Examples.Examples.Listing
{

    public static class ListingExamples
    {

        public static IRouterBuilder Create()
        {
            return DirectoryListing.From("./");
        }

    }

}
