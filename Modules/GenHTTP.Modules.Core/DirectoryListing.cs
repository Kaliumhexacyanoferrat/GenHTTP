using System.IO;

using GenHTTP.Modules.Core.Listing;

namespace GenHTTP.Modules.Core
{

    public static class DirectoryListing
    {

        public static ListingRouterBuilder From(DirectoryInfo directory) => From(directory.FullName);

        public static ListingRouterBuilder From(string directory) => new ListingRouterBuilder().Directory(directory);

    }

}
