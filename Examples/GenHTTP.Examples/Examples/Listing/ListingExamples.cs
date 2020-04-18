using GenHTTP.Api.Content;
using GenHTTP.Modules.Core;

namespace GenHTTP.Examples.Examples.Listing
{

    public static class ListingExamples
    {

        public static IHandlerBuilder Create()
        {
            return DirectoryListing.From("./");
        }

    }

}
