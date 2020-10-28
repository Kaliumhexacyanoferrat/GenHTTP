using System.IO;

using GenHTTP.Modules.DirectoryBrowsing.Provider;

namespace GenHTTP.Modules.DirectoryBrowsing
{

    public static class DirectoryListing
    {
        
        // todo: resource trees!

        public static ListingRouterBuilder From(DirectoryInfo directory) => From(directory.FullName);

        public static ListingRouterBuilder From(string directory) => new ListingRouterBuilder().Directory(directory);

    }

}
